using DMD.APPLICATION.Appointment.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Entities.Appointment;
using DMD.DOMAIN.Enums.Appointment;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.Appointment.Commands.Create
{
    [JsonSchema("AppointmentCreateCommand")]
    public class Command : IRequest<Response>
    {
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
                if (string.IsNullOrWhiteSpace(request.PatientInfoId))
                    return new BadRequestResponse("Patient is required.");

                if (request.AppointmentDateFrom >= request.AppointmentDateTo)
                    return new BadRequestResponse("Appointment end time must be later than the start time.");

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
                            x.Status != AppointmentStatus.Cancelled &&
                            x.AppointmentDateFrom < request.AppointmentDateTo &&
                            x.AppointmentDateTo > request.AppointmentDateFrom,
                        cancellationToken);

                if (hasConflict)
                    return new BadRequestResponse("Appointment schedule conflicts with an existing appointment.");

                var newItem = new AppointmentRequest
                {
                    PatientInfoId = request.PatientInfoId.Trim(),
                    AppointmentDateFrom = request.AppointmentDateFrom,
                    AppointmentDateTo = request.AppointmentDateTo,
                    ReasonForVisit = request.ReasonForVisit?.Trim() ?? string.Empty,
                    Status = status,
                    Remarks = request.Remarks?.Trim() ?? string.Empty
                };

                dbContext.AppointmentRequests.Add(newItem);
                await dbContext.SaveChangesAsync(cancellationToken);

                var patientName = string.Join(" ", new[] { patient.FirstName, patient.MiddleName }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim()));

                var response = new AppointmentModel
                {
                    Id = newItem.Id,
                    PatientInfoId = newItem.PatientInfoId,
                    AppointmentDateFrom = newItem.AppointmentDateFrom,
                    AppointmentDateTo = newItem.AppointmentDateTo,
                    ReasonForVisit = newItem.ReasonForVisit,
                    Status = newItem.Status.ToString(),
                    Remarks = newItem.Remarks,
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
