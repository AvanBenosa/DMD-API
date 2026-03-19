namespace DMD.DOMAIN.Entities.UserProfile
{
    public class PublicAppointmentEmailVerification : BaseEntity<int>
    {
        public int ClinicProfileId { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? ConsumedAtUtc { get; set; }
        public DateTime? LastSentAtUtc { get; set; }
    }
}
