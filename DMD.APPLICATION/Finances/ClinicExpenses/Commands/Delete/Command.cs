using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;
using System.Security.Claims;

namespace DMD.APPLICATION.Finances.ClinicExpenses.Commands.Delete
{
    [JsonSchema("DeleteClinicExpensesCommand")]
    public class Command : IRequest<Response>
    {
        public string Id { get; set; } = string.Empty;
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
                    return new BadRequestResponse("Expense ID is required.");
                }

                var itemId = await protectionProvider.DecryptIntIdAsync(
                    request.Id,
                    ProtectedIdPurpose.ClinicExpense);

                var item = await dbContext.ClinicExpenses
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == itemId && x.ClinicProfileId == clinicId,
                        cancellationToken);

                if (item == null)
                {
                    return new BadRequestResponse("Expense record was not found.");
                }

                dbContext.ClinicExpenses.Remove(item);
                await dbContext.SaveChangesAsync(cancellationToken);

                return new SuccessResponse<bool>(true);
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
