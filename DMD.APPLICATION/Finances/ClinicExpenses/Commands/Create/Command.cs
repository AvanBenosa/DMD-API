using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.Finances.ClinicExpenses.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Entities.FInances;
using DMD.DOMAIN.Enums;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;
using System.Security.Claims;
using ClinicExpensesEntity = DMD.DOMAIN.Entities.FInances.ClinicExpenses;

namespace DMD.APPLICATION.Finances.ClinicExpenses.Commands.Create
{
    [JsonSchema("CreateClinicExpensesCommand")]
    public class Command : IRequest<Response>
    {
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

                if (!Enum.IsDefined(typeof(ExpenseCategory), request.Category))
                {
                    return new BadRequestResponse("Expense category is required.");
                }

                if (request.Amount < 0)
                {
                    return new BadRequestResponse("Amount cannot be negative.");
                }

                var clinicExists = await dbContext.ClinicProfiles
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.Id == clinicId, cancellationToken);

                if (!clinicExists)
                {
                    return new BadRequestResponse("Clinic profile was not found.");
                }

                var newItem = new ClinicExpensesEntity
                {
                    ClinicProfileId = clinicId,
                    Remarks = request.Remarks?.Trim() ?? string.Empty,
                    Category = request.Category,
                    Date = request.Date?.Date ?? DateTime.Today,
                    Amount = request.Amount
                };

                dbContext.ClinicExpenses.Add(newItem);
                await dbContext.SaveChangesAsync(cancellationToken);

                return new SuccessResponse<ClinicExpensesModel>(await MapItemAsync(newItem));
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }

        private async Task<ClinicExpensesModel> MapItemAsync(ClinicExpensesEntity item)
        {
            return new ClinicExpensesModel
            {
                Id = await protectionProvider.EncryptIntIdAsync(item.Id, ProtectedIdPurpose.ClinicExpense) ?? string.Empty,
                ClinicProfileId = await protectionProvider.EncryptIntIdAsync(item.ClinicProfileId, ProtectedIdPurpose.Clinic) ?? string.Empty,
                Remarks = item.Remarks,
                Category = item.Category,
                Date = item.Date,
                Amount = item.Amount
            };
        }
    }
}
