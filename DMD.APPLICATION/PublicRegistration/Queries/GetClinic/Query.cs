using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.PublicRegistration.Models;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DMD.APPLICATION.PublicRegistration.Queries.GetClinic
{
    public class Query : IRequest<Response>
    {
        public string ClinicId { get; set; } = string.Empty;
    }

    public class QueryHandler : IRequestHandler<Query, Response>
    {
        private readonly DmdDbContext dbContext;
        private readonly IProtectionProvider protectionProvider;

        public QueryHandler(DmdDbContext dbContext, IProtectionProvider protectionProvider)
        {
            this.dbContext = dbContext;
            this.protectionProvider = protectionProvider;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ClinicId))
                    return new BadRequestResponse("Clinic id is required.");

                var clinicId = await protectionProvider.DecryptNullableIntIdAsync(
                    request.ClinicId,
                    ProtectedIdPurpose.Clinic);

                if (!clinicId.HasValue)
                    return new BadRequestResponse("Clinic was not found.");

                var clinic = await dbContext.ClinicProfiles
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == clinicId.Value, cancellationToken);

                if (clinic == null)
                    return new BadRequestResponse("Clinic was not found.");

                return new SuccessResponse<PublicClinicRegistrationContextModel>(
                    new PublicClinicRegistrationContextModel
                    {
                        ClinicId = request.ClinicId,
                        ClinicName = clinic.ClinicName,
                        BannerImagePath = clinic.BannerImagePath,
                        IsLocked = clinic.IsLocked,
                    });
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
