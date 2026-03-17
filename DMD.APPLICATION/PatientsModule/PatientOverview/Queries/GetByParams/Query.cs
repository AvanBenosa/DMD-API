using AutoMapper;
using DMD.APPLICATION.PatientsModule.PatientProgressNotes.Models;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.PatientsModule.PatientOverview.Queries.GetByParams
{
    [JsonSchema("GetByParamQuery")]
    public class Query : IRequest<Response>
    {
        public int Id { get; set;  }
        public int PatientInfoId { get; set; }
    }
    public class QueryHandler : IRequestHandler<Query, Response>
    {
        private readonly IMapper mapper;
        private readonly DmdDbContext dbContext;

        public QueryHandler(DmdDbContext dbContext, IMapper mapper)
        {   
            this.mapper = mapper;
            this.dbContext = dbContext;
        }
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var response = new List<PatientProgressNoteModel>();
                var items = await dbContext.PatientProgressNotes.AsNoTracking()
                    .Where(x => x.PatientInfoId == request.PatientInfoId)
                    .Select(x => mapper.Map<PatientProgressNoteModel>(x))
                    .ToListAsync();

                if (items.Any())
                {
                    items.ForEach(x =>
                    {
                        var item = mapper.Map<PatientProgressNoteModel>(x);
                        response.Add(item);
                    });
                }

                return new SuccessResponse<List<PatientProgressNoteModel>>(response);

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
