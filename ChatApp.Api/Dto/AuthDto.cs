using ChatApp.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Api.Dto
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "User name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "User name must be between 3 and 50 characters.")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public required string Password { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }
    }

    public class LoginRequestDto
    {
        [Required(ErrorMessage = "LoginName is required.")]
        public required string LoginName { get; set; } // Username or Email
        [Required(ErrorMessage = "Password is required.")]
        public required string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public required AuthUserResponseDto user { get; set; }
        public required string AccessToken { get; set; }
        public required DateTime AccessTokenExp { get; set; }
    }

    public class AuthUserResponseDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
