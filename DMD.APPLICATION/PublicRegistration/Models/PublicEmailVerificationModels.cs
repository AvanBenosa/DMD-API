namespace DMD.APPLICATION.PublicRegistration.Models
{
    public class PublicEmailVerificationCodeResponseModel
    {
        public string Email { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
    }

    public class PublicEmailVerificationStatusModel
    {
        public string Email { get; set; } = string.Empty;
        public bool Verified { get; set; }
    }
}
