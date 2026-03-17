using DMD.APPLICATION.Appointment.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Enums.Appointment;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.Appointment.Commands.Update
{
    [JsonSchema("AppointmentUpdateCommand")]
    public class Command : IRequest<Response>
    {
        public int Id { get; set; }
        public string PatientInfoId { get; set; } = string.Empty;
        public DateTime AppointmentDateFrom { get; set; }
        public DateTime AppointmentDateTo { get; set; }
        public string ReasonForVisit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private readonly DmdDbContext dbContext;

        public CommandHandler(DmdDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.AppointmentDateFrom >= request.AppointmentDateTo)
                    return new BadRequestResponse("Appointment end time must be later than the start time.");

                var item = await dbContext.AppointmentRequests
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (item == null)
                    return new BadRequestResponse("Item may have been modified or removed.");

                var patient = await dbContext.PatientInfos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id.ToString() == request.PatientInfoId, cancellationToken);

                if (patient == null)
                    return new BadRequestResponse("Selected patient does not exist.");

                if (!Enum.TryParse(request.Status, true, out AppointmentStatus status))
                    status = AppointmentStatus.Scheduled;

                var hasConflict = await dbContext.AppointmentRequests
                    .AsNoTracking()
                    .AnyAsync(
                        x =>
                            x.Id != request.Id &&
                            x.Status != AppointmentStatus.Cancelled &&
                            x.AppointmentDateFrom < request.AppointmentDateTo &&
                            x.AppointmentDateTo > request.AppointmentDateFrom,
                        cancellationToken);

                if (hasConflict)
                    return new BadRequestResponse("Appointment schedule conflicts with an existing appointment.");

                item.PatientInfoId = request.PatientInfoId.Trim();
                item.AppointmentDateFrom = request.AppointmentDateFrom;
                item.AppointmentDateTo = request.AppointmentDateTo;
                item.ReasonForVisit = request.ReasonForVisit?.Trim() ?? string.Empty;
                item.Status = status;
                item.Remarks = request.Remarks?.Trim() ?? string.Empty;

                await dbContext.SaveChangesAsync(cancellationToken);

                var patientName = string.Join(" ", new[] { patient.FirstName, patient.MiddleName }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim()));

                var response = new AppointmentModel
                {
                    Id = item.Id,
                    PatientInfoId = item.PatientInfoId,
                    AppointmentDateFrom = item.AppointmentDateFrom,
                    AppointmentDateTo = item.AppointmentDateTo,
                    ReasonForVisit = item.ReasonForVisit,
                    Status = item.Status.ToString(),
                    Remarks = item.Remarks,
                    PatientNumber = patient.PatientNumber ?? string.Empty,
                    PatientName = string.IsNullOrWhiteSpace(patient.LastName)
                        ? patientName
                        : string.IsNullOrWhiteSpace(patientName)
                            ? patient.LastName
                            : $"{patient.LastName}, {patientName}"
                };

                return new SuccessResponse<AppointmentModel>(response);
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
