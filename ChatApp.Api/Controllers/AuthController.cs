using ChatApp.Api.Data;
using ChatApp.Api.Dto;
using ChatApp.Api.Models;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly ChatAppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(ChatAppDbContext context, ILogger<AuthController> logger, TokenService tokenService)
        {
            _context = context;
            _logger = logger;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto registerRequestDto)
        {
            try
            {
                // Checked duplicate username/email
                var existedUser = await _context.Users.AnyAsync(u => u.Username == registerRequestDto.Username || u.Email == registerRequestDto.Email);

                if (existedUser)
                {
                    return Conflict("Username hoặc email đã tồn tại!");
                }

                var user = new User()
                {
                    Username = registerRequestDto.Username,
                    Email = registerRequestDto.Email
                };

                var passwordHasher = new PasswordHasher<User>();
                var hashedPassword = passwordHasher.HashPassword(user, registerRequestDto.Password);
                user.PasswordHash = hashedPassword;

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return Ok("Đăng ký tài khoản thành công!");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Lỗi khi đăng ký tài khoản {Environment.NewLine} {ex}", "AuthExceptionLog");
                return Conflict("Đăng ký không thành công, vui lòng thử lại");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginRequestDto)
        {
            try
            {
                string errMsg = "Tên đăng nhập hoặc mật khẩu không đúng";

                var existedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequestDto.LoginName || u.Email == loginRequestDto.LoginName);

                if (existedUser == null)
                {
                    return Unauthorized(errMsg);
                }

                var passwordHasher = new PasswordHasher<User>();
                bool passwordValid = passwordHasher.VerifyHashedPassword(existedUser, existedUser.PasswordHash, loginRequestDto.Password) != PasswordVerificationResult.Failed;

                if (!passwordValid)
                {
                    return Unauthorized(errMsg);
                }

                var (accessToken, expiration) = _tokenService.GenerateAccessToken(existedUser);

                return Ok(new AuthResponseDto()
                {
                    user = new AuthUserResponseDto()
                    {
                        Username = existedUser.Username,
                        Email = existedUser.Email,
                        AvatarUrl = existedUser.AvatarUrl
                    },
                    AccessToken = accessToken,
                    AccessTokenExp = expiration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi đăng nhập {Environment.NewLine} {ex}", "AuthExceptionLog");
                return BadRequest("Đăng nhập không thành công, vui lòng thử lại");
            }
        }
    }
}
