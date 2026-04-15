using System.ComponentModel.DataAnnotations;

namespace dienlanh.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = "";

        // 🔥 NÊN THÊM (quan trọng cho UI)
        public string? Name { get; set; }

        public string? Phone { get; set; }

        // Comma-separated or free-text list of equipment specializations.
        public string? Specializations { get; set; }
    }
}