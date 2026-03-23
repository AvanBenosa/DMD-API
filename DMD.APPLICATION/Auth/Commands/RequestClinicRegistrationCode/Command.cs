using DMD.APPLICATION.Auth.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Entities.UserProfile;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.Email;
using DMD.SERVICES.Email.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;
using System.Net;
using System.Security.Cryptography;

namespace DMD.APPLICATION.Auth.Commands.RequestClinicRegistrationCode
{
    [JsonSchema("RequestClinicRegistrationCodeCommand")]
    public class Command : IRequest<Response>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private const int VerificationCodeExpiryMinutes = 10;
        private readonly DmdDbContext dbContext;
        private readonly IEmailService emailService;

        public CommandHandler(DmdDbContext dbContext, IEmailService emailService)
        {
            this.dbContext = dbContext;
            this.emailService = emailService;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var email = request.Email.Trim();
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new BadRequestResponse("Email address is required.");
                }

                try
                {
                    _ = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    return new BadRequestResponse("Enter a valid email address.");
                }

                var hasExistingUser = await dbContext.UserProfiles
                    .IgnoreQueryFilters()
                    .AnyAsync(
                        item => item.Email == email
                            || item.EmailAddress == email
                            || item.UserName == email,
                        cancellationToken);

                if (hasExistingUser)
                {
                    return new BadRequestResponse("An account already exists for this email address.");
                }

                var now = DateTime.UtcNow;
                var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

                var existingCodes = await dbContext.ClinicRegistrationVerifications
                    .Where(item => item.EmailAddress == email && item.ConsumedAtUtc == null)
                    .ToListAsync(cancellationToken);

                foreach (var existingCode in existingCodes)
                {
                    existingCode.ConsumedAtUtc = now;
                }

                var verification = new ClinicRegistrationVerification
                {
                    EmailAddress = email,
                    Code = code,
                    ExpiresAtUtc = now.AddMinutes(VerificationCodeExpiryMinutes),
                    LastSentAtUtc = now
                };

                dbContext.ClinicRegistrationVerifications.Add(verification);
                await dbContext.SaveChangesAsync(cancellationToken);

                await emailService.SendAsync(
                    new PatientEmailJobRequest
                    {
                        RecipientEmail = email,
                        Subject = "OralSync registration verification code",
                        Body = BuildVerificationEmailHtml(email, code),
                        IsBodyHtml = true
                    },
                    cancellationToken);

                return new SuccessResponse<VerificationCodeResponse>(new VerificationCodeResponse
                {
                    Email = email,
                    ExpiresInMinutes = VerificationCodeExpiryMinutes
                });
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }

        private static string BuildVerificationEmailHtml(string email, string code)
        {
            var safeEmail = WebUtility.HtmlEncode(email);
            var safeCode = WebUtility.HtmlEncode(code);

            return $@"
<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>OralSync Verification Code</title>
  </head>
  <body style=""margin:0;padding:0;background-color:#eef3f8;font-family:Arial,'Helvetica Neue',Helvetica,sans-serif;color:#19324d;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""background-color:#eef3f8;padding:32px 16px;"">
      <tr>
        <td align=""center"">
          <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:640px;background:#ffffff;border:1px solid #d9e3ee;border-radius:24px;overflow:hidden;box-shadow:0 18px 42px rgba(23,53,84,0.12);"">
            <tr>
              <td style=""padding:40px 36px 24px;background:linear-gradient(180deg,#f9fbfd 0%,#f1f6fb 100%);text-align:center;"">
                <div style=""display:inline-block;padding:8px 14px;border-radius:999px;background:#e8f1fb;color:#2d6dab;font-size:12px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;"">
                  OralSync
                </div>
                <h1 style=""margin:18px 0 10px;font-size:30px;line-height:1.15;color:#15395f;"">Clinic Registration Verification</h1>
                <p style=""margin:0 auto;max-width:440px;font-size:15px;line-height:1.7;color:#59708a;"">
                  Use the verification code below to continue creating your clinic account.
                </p>
              </td>
            </tr>
            <tr>
              <td style=""padding:0 36px 36px;"">
                <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border:1px solid #dbe5ef;border-radius:22px;background:linear-gradient(180deg,#ffffff 0%,#f7fafc 100%);"">
                  <tr>
                    <td style=""padding:28px 24px;text-align:center;"">
                      <p style=""margin:0 0 8px;font-size:14px;line-height:1.6;color:#6b8097;"">
                        Verification requested for
                      </p>
                      <p style=""margin:0 0 22px;font-size:16px;font-weight:700;line-height:1.6;color:#22486d;"">
                        {safeEmail}
                      </p>
                      <div style=""display:inline-block;padding:18px 28px;border:1px dashed #bfd0e2;border-radius:18px;background:#f8fbfe;color:#102b4c;font-size:40px;font-weight:800;letter-spacing:0.32em;"">
                        {safeCode}
                      </div>
                      <p style=""margin:22px 0 0;font-size:14px;font-weight:700;line-height:1.6;color:#d63c2f;"">
                        This code expires in {VerificationCodeExpiryMinutes} minutes.
                      </p>
                    </td>
                  </tr>
                </table>
                <p style=""margin:24px 0 0;font-size:14px;line-height:1.8;color:#698097;text-align:center;"">
                  If you did not request this verification code, you can safely ignore this email.
                </p>
              </td>
            </tr>
            <tr>
              <td style=""padding:18px 24px;border-top:1px solid #e6edf5;background:#fbfcfd;text-align:center;font-size:12px;line-height:1.6;color:#7f92a5;"">
                &copy; {DateTime.UtcNow.Year} OralSync. All rights reserved.
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
        }
    }
}
