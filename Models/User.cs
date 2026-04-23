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

        // Technician profile information
        public string? Address { get; set; }
        public string? WorkStartHour { get; set; } = "08:00"; // Default 08:00
        public string? WorkEndHour { get; set; } = "19:00"; // Default 19:00
        public int? TotalJobsCompleted { get; set; } = 0;
        public decimal? AverageRating { get; set; } = 0m;
        public bool IsAvailable { get; set; } = true;
    }
}