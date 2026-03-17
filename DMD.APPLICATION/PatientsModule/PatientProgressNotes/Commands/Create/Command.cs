using AutoMapper;
using DMD.APPLICATION.PatientsModule.PatientProgressNotes.Models;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using MediatR;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.PatientsModule.PatientProgressNotes.Commands.Create
{
    [JsonSchema("CreateCommand")]
    public class Command : IRequest<Response>
    {
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
                var newItem = new DOMAIN.Entities.Patients.PatientProgressNote
                {
                    PatientInfoId = request.PatientInfoId,
                    Date = request.Date,
                    Procedure = request.Procedure,
                    Balance = request.Balance,
                    Amount = request.Amount,
                    TotalAmountDue = request.TotalAmountDue,
                    Remarks = request.Remarks,
                    Category= request.Category,
                    Account = request.Account,
                    Discount = request.Discount,
                    AmountPaid = request.AmountPaid,
                };

                dbContext.PatientProgressNotes.Add(newItem);
                await dbContext.SaveChangesAsync();

                var response = mapper.Map<PatientProgressNoteModel>(newItem);
                return new SuccessResponse<PatientProgressNoteModel>(response);
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
