namespace DMD.APPLICATION.PublicRegistration.Models
{
    public class PublicPatientAppointmentRegistrationModel
    {
        public string ClinicId { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string PatientNumber { get; set; } = string.Empty;
        public string AppointmentId { get; set; } = string.Empty;
        public DateTime AppointmentDateFrom { get; set; }
        public DateTime AppointmentDateTo { get; set; }
    }
}
