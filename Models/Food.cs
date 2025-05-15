using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingHallAPI.Models
{
    public class Food
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; } = 0;

        [Required]
        [ForeignKey(nameof(WeddingHall))]
        public int WeddingHallId { get; set; }
    }
}
