using System.ComponentModel.DataAnnotations;

namespace WeddingHallAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;

        // OTP fields
        public string? OtpCode { get; set; } = string.Empty;
        public int OtpAttempts { get; set; } = 0;
        public DateTime? OtpExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    }
}
