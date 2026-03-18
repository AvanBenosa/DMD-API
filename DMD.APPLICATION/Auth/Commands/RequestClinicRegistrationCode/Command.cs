using DMD.APPLICATION.Auth.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Entities.UserProfile;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.Email;
using DMD.SERVICES.Email.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;
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
                        Subject = "DMD clinic registration verification code",
                        Body =
                            $"Your DMD clinic registration verification code is {code}. " +
                            $"It expires in {VerificationCodeExpiryMinutes} minutes."
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
    }
}
