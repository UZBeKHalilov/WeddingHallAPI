using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WeddingHallAPI.Data;
using WeddingHallAPI.DTOs;
using WeddingHallAPI.Models;
using WeddingHallAPI.Services;


namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(WeddingHallDbContext context, IEmailOtpService emailOtpService, IOptions<AuthSettings> authSettings) : ControllerBase
    {
        private readonly WeddingHallDbContext _context = context;
        private readonly IEmailOtpService _emailOtpService = emailOtpService;

        private readonly AuthSettings _authSettings = authSettings.Value;

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

            var accessToken = GenerateJwtToken(user);
            return Ok(new {user.Email, user.FullName, accessToken});
        }

        [HttpGet("get-user/{email}")]
        public async Task<IActionResult> GetUser(string email)
        {            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found.");
            return Ok(user);
        }

        [HttpPut("update-user/{email}")]
        public async Task<IActionResult> UpdateUser(string email, UserUpdateDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found.");
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();            
            return Ok(user);
        }

        [HttpDelete("delete-user/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound("User not found.");
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();            
            return Ok("User deleted successfully.");
        }

        [Authorize]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers(string password)
        {
            if (password != "KHalilov0548")
                return Unauthorized("Invalid password.");
            
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // Helper method to generate a random 6-digit OTP
        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_authSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                }),

                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
