using System.ComponentModel.DataAnnotations;

namespace dienlanh.Models
{
    public class RepairRequest
    {
        [Key]
        public int Id { get; set; }

        // 🔥 Kỹ thuật viên
        public int? TechnicianId { get; set; }

        public User? Technician { get; set; } // ✅ phải có dấu ?

        // 🔥 Trạng thái
        public string? Status { get; set; }

        // 🔥 Thông tin yêu cầu
        public string? DeviceType { get; set; }
        public string? Issue { get; set; }
        public string? Address { get; set; }

        // 🔥 NÊN THÊM (để đúng yêu cầu đề bài)
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public string? ImagePath { get; set; }

        public int? CustomerId { get; set; }
    }
}