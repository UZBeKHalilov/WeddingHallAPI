using System.ComponentModel.DataAnnotations;

namespace WeddingHallAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
    }
}
