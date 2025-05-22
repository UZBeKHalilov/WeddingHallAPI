using System.ComponentModel.DataAnnotations;

namespace WeddingHallAPI.DTOs
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
