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
        public DateTime? PreferredVisitAt { get; set; }

        // 🔥 NÊN THÊM (để đúng yêu cầu đề bài)
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public string? ImagePath { get; set; }

        public int? CustomerId { get; set; }

        // Technician selected replaced parts and final amount.
        public string? ReplacedParts { get; set; }
        public decimal? FinalAmount { get; set; }

        // Customer confirmation / feedback.
        public bool PartsConfirmedByCustomer { get; set; }
        public bool CustomerReported { get; set; }
        public string? CustomerReportNote { get; set; }
        public DateTime? ReportedAt { get; set; }
        public bool ReportResolved { get; set; }
        public string? ReportResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Customer rating for technician after payment.
        [Range(1, 5)]
        public int? TechnicianRating { get; set; }
        public string? TechnicianRatingComment { get; set; }
        public DateTime? TechnicianRatedAt { get; set; }
    }
}