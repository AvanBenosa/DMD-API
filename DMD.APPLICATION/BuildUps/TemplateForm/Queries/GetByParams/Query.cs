using DMD.APPLICATION.BuildUps.TemplateForm.Models;
using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;
using System.Security.Claims;

namespace DMD.APPLICATION.BuildUps.TemplateForm.Queries.GetByParams
{
    [JsonSchema("GetTemplateFormByParamsQuery")]
    public class Query : IRequest<Response>
    {
        public string? ClinicId { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, Response>
    {
        private readonly DmdDbContext dbContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IProtectionProvider protectionProvider;

        public QueryHandler(
            DmdDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IProtectionProvider protectionProvider)
        {
            this.dbContext = dbContext;
            this.httpContextAccessor = httpContextAccessor;
            this.protectionProvider = protectionProvider;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var clinicId = await protectionProvider.DecryptNullableIntIdAsync(
                    request.ClinicId,
                    ProtectedIdPurpose.Clinic);

                if (!clinicId.HasValue)
                {
                    var clinicIdValue = httpContextAccessor.HttpContext?.User.FindFirstValue("clinicId");
                    clinicId = int.TryParse(clinicIdValue, out var currentClinicId) ? currentClinicId : null;
                }

                if (!clinicId.HasValue)
                {
                    return new BadRequestResponse("Authenticated clinic was not found.");
                }

                var items = await dbContext.FormTemplates
                    .AsNoTracking()
                    .Where(x => x.ClinicProfileId == clinicId.Value)
                    .OrderBy(x => x.TemplateName)
                    .ToListAsync(cancellationToken);

                var responseItems = new List<TemplateFormModel>();

                foreach (var item in items)
                {
                    responseItems.Add(new TemplateFormModel
                    {
                        Id = await protectionProvider.EncryptIntIdAsync(item.Id, ProtectedIdPurpose.FormTemplate) ?? string.Empty,
                        ClinicProfileId = await protectionProvider.EncryptIntIdAsync(item.ClinicProfileId, ProtectedIdPurpose.Clinic) ?? string.Empty,
                        TemplateName = item.TemplateName,
                        TemplateContent = item.TemplateContent,
                        Date = item.Date
                    });
                }

                return new SuccessResponse<TemplateFormResponseModel>(new TemplateFormResponseModel
                {
                    Items = responseItems,
                    TotalCount = responseItems.Count
                });
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
