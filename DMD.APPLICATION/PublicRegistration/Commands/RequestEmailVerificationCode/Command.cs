using DMD.APPLICATION.PublicRegistration.Models;
using DMD.APPLICATION.Responses;
using DMD.APPLICATION.Common.ProtectedIds;
using DMD.DOMAIN.Entities.UserProfile;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.Email;
using DMD.SERVICES.Email.Models;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;
using System.Security.Cryptography;

namespace DMD.APPLICATION.PublicRegistration.Commands.RequestEmailVerificationCode
{
    [JsonSchema("RequestPublicAppointmentEmailVerificationCodeCommand")]
    public class Command : IRequest<Response>
    {
        public string ClinicId { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private const int VerificationCodeExpiryMinutes = 10;
        private readonly DmdDbContext dbContext;
        private readonly IEmailService emailService;
        private readonly IProtectionProvider protectionProvider;

        public CommandHandler(
            DmdDbContext dbContext,
            IEmailService emailService,
            IProtectionProvider protectionProvider)
        {
            this.dbContext = dbContext;
            this.emailService = emailService;
            this.protectionProvider = protectionProvider;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ClinicId))
                {
                    return new BadRequestResponse("Clinic id is required.");
                }

                var email = request.EmailAddress.Trim();
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

                var clinicId = await protectionProvider.DecryptNullableIntIdAsync(
                    request.ClinicId,
                    ProtectedIdPurpose.Clinic);

                if (!clinicId.HasValue)
                {
                    return new BadRequestResponse("Clinic was not found.");
                }

                var clinic = await dbContext.ClinicProfiles
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == clinicId.Value, cancellationToken);

                if (clinic == null)
                {
                    return new BadRequestResponse("Clinic was not found.");
                }

                if (clinic.IsLocked)
                {
                    return new BadRequestResponse("This clinic is not accepting appointment registrations right now.");
                }

                var now = DateTime.UtcNow;
                var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

                var existingCodes = await dbContext.PublicAppointmentEmailVerifications
                    .Where(item =>
                        item.ClinicProfileId == clinicId.Value &&
                        item.EmailAddress == email &&
                        item.ConsumedAtUtc == null)
                    .ToListAsync(cancellationToken);

                foreach (var existingCode in existingCodes)
                {
                    existingCode.ConsumedAtUtc = now;
                }

                var verification = new PublicAppointmentEmailVerification
                {
                    ClinicProfileId = clinicId.Value,
                    EmailAddress = email,
                    Code = code,
                    ExpiresAtUtc = now.AddMinutes(VerificationCodeExpiryMinutes),
                    LastSentAtUtc = now,
                };

                dbContext.PublicAppointmentEmailVerifications.Add(verification);
                await dbContext.SaveChangesAsync(cancellationToken);

                await emailService.SendAsync(
                    new PatientEmailJobRequest
                    {
                        RecipientEmail = email,
                        Subject = "DMD appointment email verification code",
                        Body =
                            $"Your DMD appointment verification code is {code}. " +
                            $"It expires in {VerificationCodeExpiryMinutes} minutes."
                    },
                    cancellationToken);

                return new SuccessResponse<PublicEmailVerificationCodeResponseModel>(
                    new PublicEmailVerificationCodeResponseModel
                    {
                        Email = email,
                        ExpiresInMinutes = VerificationCodeExpiryMinutes
                    });
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
            finally
            {
                await dbContext.DisposeAsync();
            }
        }
    }
}
