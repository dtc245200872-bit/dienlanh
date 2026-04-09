using System.ComponentModel.DataAnnotations;

namespace dienlanh.Models
{
    public class Component
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 999999999)]
        public decimal Price { get; set; }

        public int? DeviceId { get; set; }
        public Device? Device { get; set; }
    }
}
