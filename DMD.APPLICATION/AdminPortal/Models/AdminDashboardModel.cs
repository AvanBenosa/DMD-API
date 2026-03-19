namespace DMD.APPLICATION.AdminPortal.Models
{
    public class AdminDashboardPatientTrendClinicModel
    {
        public string ClinicName { get; set; } = string.Empty;
        public int PatientCount { get; set; }
    }

    public class AdminDashboardPatientTrendModel
    {
        public string Date { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public List<AdminDashboardPatientTrendClinicModel> Clinics { get; set; } = new();
    }

    public class AdminDashboardOwnerModel
    {
        public string Name { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class AdminClinicDashboardItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public int DoctorCount { get; set; }
        public int PatientCount { get; set; }
        public bool IsLocked { get; set; }
        public bool IsDataPrivacyAccepted { get; set; }
    }

    public class AdminDashboardModel
    {
        public int TotalClinics { get; set; }
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public List<AdminClinicDashboardItemModel> Clinics { get; set; } = new();
        public List<AdminDashboardPatientTrendModel> DailyPatientTrends { get; set; } = new();
        public List<AdminDashboardOwnerModel> Owners { get; set; } = new();
    }
}
