using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;
using WeddingHallAPI.Services;
using WeddingHallAPI.DTOs;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(WeddingHallDbContext context, IEmailOtpService emailOtpService) : ControllerBase
    {
        private readonly WeddingHallDbContext _context = context;
        private readonly IEmailOtpService _emailOtpService = emailOtpService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return BadRequest("Email already registered");

            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                OtpCode = GenerateOtp(),
                OtpExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _emailOtpService.SendOtpEmail(user.Email, user.OtpCode);  // <-- send OTP

            return Ok("One-Time Password sent to your email.");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return BadRequest("One-Time Password");

            if (user.OtpAttempts >= 5)
                return BadRequest("Too many attempts. Try again later.");

            if (user.IsVerified)
                return Ok("Already confirmed.");

            if (user.OtpCode != dto.OtpCode || user.OtpExpiresAt < DateTime.UtcNow)
            {
                user.OtpAttempts++;
                await _context.SaveChangesAsync();
                return BadRequest("Invalid email or One-Time Password.");
            }

            user.IsVerified = true;
            user.OtpCode = null;
            user.OtpExpiresAt = null;
            user.OtpAttempts = 0;
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        
        
        // Helper method to generate a random 6-digit OTP
        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
