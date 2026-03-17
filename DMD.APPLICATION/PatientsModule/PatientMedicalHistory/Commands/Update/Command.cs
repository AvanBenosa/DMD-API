using AutoMapper;
using DMD.APPLICATION.PatientsModule.PatientMedicalHistory.Model;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.PatientsModule.PatientMedicalHistory.Commands.Update
{
    [JsonSchema("UpdateCommand")]
    public class Command : IRequest<Response>
    {
        public int Id { get; set; }
        public int PatientInfoId { get; set; }
        public DateTime? Date { get; set; }
        public bool Q1 { get; set; }
        public bool Q2 { get; set; }
        public bool Q3 { get; set; }
        public bool Q4 { get; set; }
        public bool Q5 { get; set; }
        public bool Q6 { get; set; }
        public bool Q7 { get; set; }
        public bool Q8 { get; set; }
        public bool Q9 { get; set; }
        public bool Q10Nursing { get; set; }
        public bool Q10Pregnant { get; set; }
        public bool Q11 { get; set; }
        public bool Q12 { get; set; }
        public bool Q13 { get; set; }
        public string Others { get; set; }
        public string Remarks { get; set; }
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
                var item = await dbContext.PatientMedicalHistories.FirstOrDefaultAsync(x => x.Id == request.Id && x.PatientInfoId == request.PatientInfoId);

                if (item == null)
                    return new BadRequestResponse("Item may have been modified or removed.");

                item.Date = request.Date;
                item.Q1 = request.Q1;
                item.Q2 = request.Q2;
                item.Q3 = request.Q3;
                item.Q4 = request.Q4;
                item.Q5 = request.Q5;
                item.Q6 = request.Q6;
                item.Q7 = request.Q7;
                item.Q8 = request.Q8;
                item.Q9 = request.Q9;
                item.Q10Nursing = request.Q10Nursing;
                item.Q10Pregnant = request.Q10Pregnant;
                item.Q11 = request.Q11;
                item.Q12 = request.Q12;
                item.Q13 = request.Q13;
                item.Others = request.Others;
                item.Remarks = request.Remarks;

                await dbContext.SaveChangesAsync();
                await dbContext.DisposeAsync();

                var response = mapper.Map<PatientMedicalHistoryModel>(item);

                return new SuccessResponse<PatientMedicalHistoryModel>(response);

            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
