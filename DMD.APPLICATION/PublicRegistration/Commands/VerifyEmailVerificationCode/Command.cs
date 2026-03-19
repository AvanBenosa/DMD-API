using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.PublicRegistration.Models;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.PublicRegistration.Commands.VerifyEmailVerificationCode
{
    [JsonSchema("VerifyPublicAppointmentEmailVerificationCodeCommand")]
    public class Command : IRequest<Response>
    {
        public string ClinicId { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private readonly DmdDbContext dbContext;
        private readonly IProtectionProvider protectionProvider;

        public CommandHandler(DmdDbContext dbContext, IProtectionProvider protectionProvider)
        {
            this.dbContext = dbContext;
            this.protectionProvider = protectionProvider;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ClinicId))
                    return new BadRequestResponse("Clinic id is required.");

                var email = request.EmailAddress.Trim();
                if (string.IsNullOrWhiteSpace(email))
                    return new BadRequestResponse("Email address is required.");

                var code = request.VerificationCode.Trim();
                if (string.IsNullOrWhiteSpace(code))
                    return new BadRequestResponse("Verification code is required.");

                var clinicId = await protectionProvider.DecryptNullableIntIdAsync(
                    request.ClinicId,
                    ProtectedIdPurpose.Clinic);

                if (!clinicId.HasValue)
                    return new BadRequestResponse("Clinic was not found.");

                var verification = await dbContext.PublicAppointmentEmailVerifications
                    .AsNoTracking()
                    .Where(item =>
                        item.ClinicProfileId == clinicId.Value &&
                        item.EmailAddress == email &&
                        item.ConsumedAtUtc == null)
                    .OrderByDescending(item => item.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (verification == null
                    || verification.ExpiresAtUtc < DateTime.UtcNow
                    || !string.Equals(verification.Code, code, StringComparison.Ordinal))
                {
                    return new BadRequestResponse("Verification code is invalid or expired.");
                }

                return new SuccessResponse<PublicEmailVerificationStatusModel>(
                    new PublicEmailVerificationStatusModel
                    {
                        Email = email,
                        Verified = true
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
