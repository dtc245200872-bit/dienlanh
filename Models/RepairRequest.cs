using System.ComponentModel.DataAnnotations;

namespace dienlanh.Models
{
    public class RepairRequest
    {
        [Key] // 👈 thêm dòng này (cực quan trọng)
        public int Id { get; set; }

        public string? DeviceType { get; set; }
        public string? Issue { get; set; }
        public string? Address { get; set; }
    }
}