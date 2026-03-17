namespace DMD.APPLICATION.Dashboard.Models
{
    public class DashboardAppointmentModel
    {
        public string Time { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool? Highlight { get; set; }
    }
}
