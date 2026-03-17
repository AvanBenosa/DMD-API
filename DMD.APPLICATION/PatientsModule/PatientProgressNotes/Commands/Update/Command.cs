using AutoMapper;
using DMD.APPLICATION.PatientsModule.PatientProgressNotes.Models;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.PatientsModule.PatientProgressNotes.Commands.Update
{
    [JsonSchema("UpdateCommand")]
    public class Command : IRequest<Response>
    {
        public int Id { get; set; }
        public int PatientInfoId { get; set; }
        public DateTime? Date { get; set; }
        public string Procedure { get; set; }
        public string Category { get; set; }
        public string Remarks { get; set; }

        public double Balance { get; set; }
        public string Account { get; set; }
        public double Amount { get; set; }
        public double Discount { get; set; }
        public double TotalAmountDue { get; set; }
        public double AmountPaid { get; set; }
    }
    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private readonly DmdDbContext dbContext;
        private readonly IMapper mapper;

        public CommandHandler(DmdDbContext dbContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
        }
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var item = await dbContext.PatientProgressNotes.FirstOrDefaultAsync(x => x.Id == request.Id && x.PatientInfoId == request.PatientInfoId);

                if (item == null)
                    return new BadRequestResponse("Item may have been modified or removed.");

                item.Date = request.Date;
                item.Procedure = request.Procedure;
                item.Balance = request.Balance;
                item.AmountPaid = request.AmountPaid;
                item.Discount = request.Discount;
                item.TotalAmountDue = request.TotalAmountDue;
                item.Remarks = request.Remarks;
                item.Category = request.Category;
                item.Discount = request.Discount;
                item.Account = request.Account;

                await dbContext.SaveChangesAsync();
                await dbContext.DisposeAsync();

                var response = mapper.Map<PatientProgressNoteModel>(item);

                return new SuccessResponse<PatientProgressNoteModel>(response);

            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
