using ChatApp.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Api.Extensions
{
    public static class ChatHubExtensions
    {
        public static Guid? GetUserId(this HubCallerContext context)
        {
            ClaimsPrincipal user = context.User;

            var userIdStr = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid parsedUserId))
            {
                return null;
            }

            return parsedUserId;
        }
    }
}
