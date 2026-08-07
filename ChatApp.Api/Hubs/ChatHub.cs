using ChatApp.Api.Data;
using ChatApp.Api.Dto;
using ChatApp.Api.Extensions;
using ChatApp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatApp.Api.Hubs
{
    [Authorize]
    public sealed class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly ChatAppDbContext _context;

        public ChatHub(ChatAppDbContext context, ILogger<ChatHub> logger)
        {
            _context = context;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.GetUserId();

                // Kiểm tra an toàn trước khi Parse Guid để tránh văng Exception không đáng có
                if (userId == null)
                {
                    // Trả lỗi định dạng rõ ràng về client
                    await Clients.Caller.SendAsync("OnConnectionError", "Thông tin định danh người dùng không hợp lệ hoặc thiếu.");
                    Context.Abort(); // Ngắt kết nối ngay lập tức
                    return;
                }

                // Tìm kiếm người dùng trong Database
                var existedUser = await _context.Users.FindAsync(userId);

                if (existedUser != null)
                {
                    var connectionId = Context.ConnectionId;
                    var userConnections = new UserConnection
                    {
                        Id = Guid.NewGuid(),
                        UserId = existedUser.Id,
                        ConnectionId = connectionId,
                        ConnectedAt = DateTime.UtcNow
                    };

                    await _context.UserConnections.AddAsync(userConnections);
                    await _context.SaveChangesAsync();

                    // Đổi status sang Online khi user kết nối thành công
                    await _context.Users
                        .Where(u => u.Id == userId)
                        .ExecuteUpdateAsync(u =>
                        u.SetProperty(x => x.Status, "Online"));

                    // Thêm ConnectionId vào Group riêng của user
                    await Groups.AddToGroupAsync(connectionId, $"user_{userId}");

                    bool isFirstConnection = !await _context.UserConnections
                        .AnyAsync(x => x.UserId == userId && x.ConnectionId != connectionId);

                    if (isFirstConnection)
                    {
                        // Gửi thông báo cho các bạn bè hoặc thành viên phòng chat rằng user đã online
                        var allContacts = await GetRelatedUserId(userId);

                        foreach (var contact in allContacts)
                        {
                            await Clients.Group($"user_{contact}").SendAsync("OnUserOnline", userId);
                        }
                    }

                    // Chỉ gọi base khi mọi logic nghiệp vụ của bạn đã chạy thành công
                    await base.OnConnectedAsync();
                }
                else
                {
                    // Đã sửa: Truyền đúng (MethodName, Nội dung nhắn)
                    await Clients.Caller.SendAsync("OnConnectionError", "Tài khoản người dùng không tồn tại trong hệ thống.");
                    Context.Abort(); // Ngắt kết nối vì user này không hợp lệ
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi kết nối ChatHub cho ConnectionId: {ConnectionId}", Context.ConnectionId);

                try
                {
                    // Cố gắng báo lỗi về cho Client trước khi sập hẳn
                    await Clients.Caller.SendAsync("OnConnectionError", "Kết nối thất bại do lỗi hệ thống nội bộ.");
                }
                catch
                {
                    // Bọc thêm try-catch nhỏ đề phòng trường hợp kết nối đã chết hẳn từ trước, không thể SendAsync
                }
                finally
                {
                    Context.Abort(); // Đảm bảo kết nối luôn luôn bị đóng nếu có lỗi xảy ra
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                // 1. Ghi nhận nhật ký nếu kết nối bị ngắt do lỗi đột ngột (rớt mạng, crash từ client...)
                if (exception != null)
                {
                    _logger.LogWarning(exception, "Kết nối {ConnectionId} bị ngắt đột ngột do lỗi.", connectionId);
                }
                else
                {
                    _logger.LogInformation("Kết nối {ConnectionId} đã chủ động ngắt kết nối an toàn.", connectionId);
                }

                // 2. Tìm và xóa ConnectionId này khỏi Database
                // Sử dụng ExecuteDeleteAsync (Từ .NET 7 trở lên) để xóa trực tiếp, tối ưu hiệu năng không cần nạp thực thể vào bộ nhớ
                await _context.UserConnections
                    .Where(x => x.ConnectionId == connectionId)
                    .ExecuteDeleteAsync();

                // 3. Kiểm tra nếu không còn

                var userId = Context.GetUserId();

                if (userId == null)
                {
                    return;
                }

                var existedConnections = await _context.UserConnections
                    .AnyAsync(x => x.UserId == userId);

                if (!existedConnections)
                {
                    // Nếu không còn kết nối nào của user này, bạn có thể thực hiện các hành động bổ sung, ví dụ: cập nhật trạng thái offline
                    // Hiện chỉ log ra, chưa xử lí logic
                    _logger.LogInformation("User đã offline hoàn toàn (hết connection): {ConnectionId}", connectionId);

                    // Các bước xử lí logic người dùng disconnect hoàn toàn, ví dụ: cập nhật trạng thái offline, gửi thông báo cho bạn bè, v.v.
                    // 1/ Cập nhật trạng thái người dùng trong bảng Users

                    await _context.Users
                        .Where(u => u.Id == userId)
                        .ExecuteUpdateAsync(u =>
                        u.SetProperty(x => x.LastSeenAt, DateTime.UtcNow)
                        .SetProperty(x => x.Status, "Offline"));

                    // 2/ gửi thông báo cho các bạn bè của người dùng này rằng họ đã offline (nếu cần)

                    var allContacts = await GetRelatedUserId(userId);

                    foreach (var contact in allContacts)
                    {
                        // Gửi thông báo cho từng bạn bè hoặc thành viên phòng chat rằng user đã offline
                        await Clients.Group($"user_{contact}").SendAsync("OnUserOffline", userId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu quá trình dọn dẹp database thất bại
                _logger.LogError(ex, "Lỗi khi xử lý ngắt kết nối ChatHub cho ConnectionId: {ConnectionId}", connectionId);
            }
            finally
            {
                // 3. Luôn luôn gọi base method ở cuối cùng để SignalR hoàn tất vòng đời ngắt kết nối
                await base.OnDisconnectedAsync(exception);
            }
        }

        public async Task SendMessage(SendMessageRequestDto request)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                var userId = Context.GetUserId();

                await EnsureMembership(userId, request.RoomId);

                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new HubException("Người dùng không tồn tại trong hệ thống.");
                }

                await JoinRoom(request.RoomId);

                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    SenderId = user.Id,
                    Content = request.Content,
                    ReplyToMessageId = request.ReplyToMessageId,
                    Type = request.MessageType,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Gửi tin nhắn đến tất cả các thành viên trong phòng chat
                await Clients.Group($"room_{request.RoomId}").SendAsync("ReceiveMessage", new MessageDto { 
                    Id = message.Id,
                    RoomId = message.RoomId,
                    MessageType = message.Type,
                    Content = message.Content,
                    ReplyToMessageId = message.ReplyToMessageId,
                    SentAt = message.SentAt,
                    Sender = new SenderDto
                    {
                        Id = message.SenderId,
                        Username = user.Username,
                        AvatarUrl = user.AvatarUrl
                    }
                });

            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu quá trình dọn dẹp database thất bại
                _logger.LogError(ex, "Không thể gửi tin nhắn: {ConnectionId}", connectionId);
            }
        }

        public async Task JoinRoom(Guid roomId)
        {
            var userId = Context.GetUserId();
            await EnsureMembership(userId, roomId);
            bool existedRoom = await _context.Rooms.AnyAsync(r => r.Id == roomId);
            if (!existedRoom)
            {
                throw new HubException("Phòng chat không tồn tại.");
            }
            // Thêm ConnectionId vào Group của phòng chat
            await Groups.AddToGroupAsync(Context.ConnectionId, $"room_{roomId}");
        }

        private async Task<List<Guid>> GetRelatedUserId(Guid? userId)
        {
            var friendIds = _context.Friendships
                                            .Where(f => f.RequesterId == userId || f.AddresseeId == userId)
                                            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId);

            var roomateIds = _context.RoomMembers
                                    .Where(rm => rm.UserId == userId)
                                    .SelectMany(rm => rm.Room.RoomMembers)
                                    .Where(rmm => rmm.UserId != userId)
                                    .Select(rmm => rmm.UserId);

            var allContacts = await friendIds.Union(roomateIds).Distinct().ToListAsync();

            return allContacts;
        }

        private async Task EnsureMembership(Guid? userId, Guid roomId)
        {
            var isMember = await _context.RoomMembers
                .AnyAsync(rm => rm.RoomId == roomId && rm.UserId == userId);
            if (!isMember)
            {
                throw new HubException("Người dùng không phải là thành viên của phòng chat này.");
            }
        }
    }
}
