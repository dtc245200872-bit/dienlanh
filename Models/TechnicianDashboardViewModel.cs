namespace dienlanh.Models
{
    public class TechnicianDashboardViewModel
    {
        public User TechnicianProfile { get; set; } = new();
        public List<RepairRequest> AssignedTasks { get; set; } = new();
        public List<RepairRequest> WorkHistory { get; set; } = new();
        public List<TechnicianNotification> Notifications { get; set; } = new();
        public TechnicianSchedule Schedule { get; set; } = new();
        public int TotalAssignedTasks { get; set; }
        public int TotalCompletedToday { get; set; }
        public int TotalCompletedThisMonth { get; set; }
        public int UnreadNotifications { get; set; }
    }

    public class TechnicianNotification
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = ""; // "assignment", "payment", "rating", "message"
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public int? RelatedJobId { get; set; }
    }

    public class TechnicianSchedule
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public List<ScheduleEntry> WeeklySchedule { get; set; } = new();
        public List<TimeOffRequest> TimeOffRequests { get; set; } = new();
        public DateTime? NextAvailableDate { get; set; }
    }

    public class ScheduleEntry
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan EndTime { get; set; } = new TimeSpan(19, 0, 0);
        public bool IsWorkDay { get; set; } = true;
    }

    public class TimeOffRequest
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "pending"; // pending, approved, rejected
        public string? AdminNote { get; set; }
    }
}
