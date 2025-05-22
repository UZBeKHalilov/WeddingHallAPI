using System.ComponentModel.DataAnnotations;

namespace WeddingHallAPI.Models
{
    public class WeddingHall
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }
        public string? Address { get; set; } = "Unknown Address";
        public string? ImageUrl { get; set; } = string.Empty;
        public virtual ICollection<Food>? Foods { get; set; } = new List<Food>();
    }
}
