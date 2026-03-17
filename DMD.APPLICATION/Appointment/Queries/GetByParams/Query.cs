using DMD.APPLICATION.Appointment.Models;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.Appointment.Queries.GetByParams
{
    [JsonSchema("AppointmentGetByParamQuery")]
    public class Query : IRequest<Response>
    {
        public string ClinicId { get; set; } = string.Empty;
        public string Que { get; set; } = string.Empty;
        public int PageStart { get; set; }
        public int PageEnd { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, Response>
    {
        private readonly DmdDbContext dbContext;

        public QueryHandler(DmdDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var appointments = await dbContext.AppointmentRequests
                    .AsNoTracking()
                    .OrderByDescending(x => x.AppointmentDateFrom)
                    .ToListAsync(cancellationToken);

                var patientIds = appointments
                    .Select(x => x.PatientInfoId)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                var patients = await dbContext.PatientInfos
                    .AsNoTracking()
                    .Where(x => patientIds.Contains(x.Id.ToString()))
                    .Select(x => new
                    {
                        x.Id,
                        x.PatientNumber,
                        x.FirstName,
                        x.LastName,
                        x.MiddleName
                    })
                    .ToListAsync(cancellationToken);

                var patientLookup = patients.ToDictionary(
                    x => x.Id.ToString(),
                    x =>
                    {
                        var givenNames = string.Join(" ", new[] { x.FirstName, x.MiddleName }
                            .Where(value => !string.IsNullOrWhiteSpace(value))
                            .Select(value => value.Trim()));

                        var patientName = string.IsNullOrWhiteSpace(x.LastName)
                            ? givenNames
                            : string.IsNullOrWhiteSpace(givenNames)
                                ? x.LastName
                                : $"{x.LastName}, {givenNames}";

                        return new
                        {
                            x.PatientNumber,
                            PatientName = patientName
                        };
                    });

                var items = appointments.Select(x =>
                {
                    patientLookup.TryGetValue(x.PatientInfoId ?? string.Empty, out var patient);

                    return new AppointmentModel
                    {
                        Id = x.Id,
                        PatientInfoId = x.PatientInfoId ?? string.Empty,
                        AppointmentDateFrom = x.AppointmentDateFrom,
                        AppointmentDateTo = x.AppointmentDateTo,
                        ReasonForVisit = x.ReasonForVisit ?? string.Empty,
                        Status = x.Status.ToString(),
                        Remarks = x.Remarks ?? string.Empty,
                        PatientName = patient?.PatientName ?? string.Empty,
                        PatientNumber = patient?.PatientNumber ?? string.Empty
                    };
                }).ToList();

                var response = new AppointmentResponseModel
                {
                    Items = items,
                    PageStart = request.PageStart,
                    PageEnd = request.PageEnd,
                    TotalCount = items.Count
                };

                return new SuccessResponse<AppointmentResponseModel>(response);
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
