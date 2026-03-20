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

namespace DMD.APPLICATION.BuildUps.TemplateForm.Commands.Update
{
    [JsonSchema("UpdateTemplateFormCommand")]
    public class Command : IRequest<Response>
    {
        public string Id { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateContent { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private readonly DmdDbContext dbContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IProtectionProvider protectionProvider;

        public CommandHandler(
            DmdDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IProtectionProvider protectionProvider)
        {
            this.dbContext = dbContext;
            this.httpContextAccessor = httpContextAccessor;
            this.protectionProvider = protectionProvider;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var clinicIdValue = httpContextAccessor.HttpContext?.User.FindFirstValue("clinicId");
                if (!int.TryParse(clinicIdValue, out var clinicId))
                {
                    return new BadRequestResponse("Authenticated clinic was not found.");
                }

                if (string.IsNullOrWhiteSpace(request.Id))
                {
                    return new BadRequestResponse("Template form ID is required.");
                }

                if (string.IsNullOrWhiteSpace(request.TemplateName))
                {
                    return new BadRequestResponse("Template name is required.");
                }

                if (string.IsNullOrWhiteSpace(request.TemplateContent))
                {
                    return new BadRequestResponse("Template content is required.");
                }

                var itemId = await protectionProvider.DecryptIntIdAsync(request.Id, ProtectedIdPurpose.FormTemplate);
                var item = await dbContext.FormTemplates
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.Id == itemId && x.ClinicProfileId == clinicId, cancellationToken);

                if (item == null)
                {
                    return new BadRequestResponse("Template form was not found.");
                }

                item.TemplateName = request.TemplateName.Trim();
                item.TemplateContent = request.TemplateContent.Trim();
                item.Date = request.Date ?? item.Date ?? DateTime.Now;

                await dbContext.SaveChangesAsync(cancellationToken);

                return new SuccessResponse<TemplateFormModel>(new TemplateFormModel
                {
                    Id = await protectionProvider.EncryptIntIdAsync(item.Id, ProtectedIdPurpose.FormTemplate) ?? string.Empty,
                    ClinicProfileId = await protectionProvider.EncryptIntIdAsync(item.ClinicProfileId, ProtectedIdPurpose.Clinic) ?? string.Empty,
                    TemplateName = item.TemplateName,
                    TemplateContent = item.TemplateContent,
                    Date = item.Date
                });
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
