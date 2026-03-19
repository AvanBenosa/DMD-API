namespace DMD.APPLICATION.PublicRegistration.Models
{
    public class PublicClinicRegistrationContextModel
    {
        public string ClinicId { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string BannerImagePath { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }
}
