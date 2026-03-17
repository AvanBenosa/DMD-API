namespace DMD.APPLICATION.Dashboard.Models
{
    public class DashboardPatientItemModel
    {
        public int? Id { get; set; }
        public string? PatientNumber { get; set; }
        public string? FullName { get; set; }
        public string? LatestActivity { get; set; }
    }
}
