namespace dienlanh.Models
{
    public class AdminStatisticsViewModel
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int PaidRequests { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalTechnicians { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DeviceStatsItem> DeviceBreakdown { get; set; } = new();
        public List<StatusStatsItem> StatusBreakdown { get; set; } = new();

        // Time-based statistics
        public int TodayRequests { get; set; }
        public int TodayCompletedRequests { get; set; }
        public decimal TodayRevenue { get; set; }

        public int MonthRequests { get; set; }
        public int MonthCompletedRequests { get; set; }
        public decimal MonthRevenue { get; set; }

        public int YearRequests { get; set; }
        public int YearCompletedRequests { get; set; }
        public decimal YearRevenue { get; set; }

        public string SelectedPeriod { get; set; } = "all"; // all, day, month, year
    }

    public class DeviceStatsItem
    {
        public string DeviceType { get; set; } = "Chưa rõ";
        public int Count { get; set; }
    }

    public class StatusStatsItem
    {
        public string Status { get; set; } = "Chưa cập nhật";
        public int Count { get; set; }
    }
}
