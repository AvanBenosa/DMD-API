using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.Finances.ClinicExpenses.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Enums;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;
using System.Security.Claims;

namespace DMD.APPLICATION.Finances.ClinicExpenses.Commands.Update
{
    [JsonSchema("UpdateClinicExpensesCommand")]
    public class Command : IRequest<Response>
    {
        public string Id { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public ExpenseCategory Category { get; set; }
        public DateTime? Date { get; set; }
        public double Amount { get; set; }
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

                if (!Enum.IsDefined(typeof(ExpenseCategory), request.Category))
                {
                    return new BadRequestResponse("Expense category is required.");
                }

                if (request.Amount < 0)
                {
                    return new BadRequestResponse("Amount cannot be negative.");
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

                item.Remarks = request.Remarks?.Trim() ?? string.Empty;
                item.Category = request.Category;
                item.Date = request.Date?.Date ?? item.Date;
                item.Amount = request.Amount;

                await dbContext.SaveChangesAsync(cancellationToken);

                return new SuccessResponse<ClinicExpensesModel>(new ClinicExpensesModel
                {
                    Id = await protectionProvider.EncryptIntIdAsync(item.Id, ProtectedIdPurpose.ClinicExpense) ?? string.Empty,
                    ClinicProfileId = await protectionProvider.EncryptIntIdAsync(item.ClinicProfileId, ProtectedIdPurpose.Clinic) ?? string.Empty,
                    Remarks = item.Remarks,
                    Category = item.Category,
                    Date = item.Date,
                    Amount = item.Amount
                });
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
