using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.PublicRegistration.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Entities.Appointment;
using DMD.DOMAIN.Entities.Patients;
using DMD.DOMAIN.Enums.Appointment;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.PublicRegistration.Commands.CreatePatientAppointment
{
    [JsonSchema("CreatePublicPatientAppointmentCommand")]
    public class Command : IRequest<Response>
    {
        public string ClinicId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string Religion { get; set; } = string.Empty;
        public DateTime AppointmentDateFrom { get; set; }
        public DateTime AppointmentDateTo { get; set; }
        public string ReasonForVisit { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private readonly DmdDbContext dbContext;
        private readonly IProtectionProvider protectionProvider;

        public CommandHandler(DmdDbContext dbContext, IProtectionProvider protectionProvider)
        {
            this.dbContext = dbContext;
            this.protectionProvider = protectionProvider;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ClinicId))
                    return new BadRequestResponse("Clinic id is required.");

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                    return new BadRequestResponse("Patient first name and last name are required.");

                if (request.AppointmentDateFrom >= request.AppointmentDateTo)
                    return new BadRequestResponse("Appointment end time must be later than the start time.");

                var clinicId = await protectionProvider.DecryptNullableIntIdAsync(
                    request.ClinicId,
                    ProtectedIdPurpose.Clinic);

                if (!clinicId.HasValue)
                    return new BadRequestResponse("Clinic was not found.");

                var clinic = await dbContext.ClinicProfiles
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == clinicId.Value, cancellationToken);

                if (clinic == null)
                    return new BadRequestResponse("Clinic was not found.");

                if (clinic.IsLocked)
                    return new BadRequestResponse("This clinic is not accepting appointment registrations right now.");

                var hasConflict = await (
                    from appointment in dbContext.AppointmentRequests.AsNoTracking()
                    join patient in dbContext.PatientInfos.AsNoTracking()
                        on appointment.PatientInfoId equals patient.Id.ToString()
                    where patient.ClinicProfileId == clinicId.Value
                          && appointment.Status != AppointmentStatus.Cancelled
                          && appointment.AppointmentDateFrom < request.AppointmentDateTo
                          && appointment.AppointmentDateTo > request.AppointmentDateFrom
                    select appointment.Id
                ).AnyAsync(cancellationToken);

                if (hasConflict)
                    return new BadRequestResponse("Appointment schedule conflicts with an existing appointment.");

                var today = DateTime.Today;
                var countToday = await dbContext.PatientInfos
                    .IgnoreQueryFilters()
                    .CountAsync(p => p.CreatedAt.Date == today, cancellationToken);

                var patientNumber = $"DMD-{today:yyyyMMdd}-{(countToday + 1):D4}";

                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                var newPatient = new PatientInfo
                {
                    ClinicProfileId = clinicId.Value,
                    PatientNumber = patientNumber,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    MiddleName = request.MiddleName?.Trim() ?? string.Empty,
                    EmailAddress = request.EmailAddress?.Trim() ?? string.Empty,
                    BirthDate = request.BirthDate,
                    ContactNumber = request.ContactNumber?.Trim() ?? string.Empty,
                    Address = request.Address?.Trim() ?? string.Empty,
                    Occupation = request.Occupation?.Trim() ?? string.Empty,
                    Religion = request.Religion?.Trim() ?? string.Empty,
                    ProfilePicture = string.Empty,
                };

                dbContext.PatientInfos.Add(newPatient);
                await dbContext.SaveChangesAsync(cancellationToken);

                var newAppointment = new AppointmentRequest
                {
                    PatientInfoId = newPatient.Id.ToString(),
                    AppointmentDateFrom = request.AppointmentDateFrom,
                    AppointmentDateTo = request.AppointmentDateTo,
                    ReasonForVisit = request.ReasonForVisit?.Trim() ?? string.Empty,
                    Status = AppointmentStatus.Scheduled,
                    Remarks = request.Remarks?.Trim() ?? string.Empty,
                };

                dbContext.AppointmentRequests.Add(newAppointment);
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new SuccessResponse<PublicPatientAppointmentRegistrationModel>(
                    new PublicPatientAppointmentRegistrationModel
                    {
                        ClinicId = request.ClinicId,
                        ClinicName = clinic.ClinicName,
                        PatientId = await protectionProvider.EncryptIntIdAsync(newPatient.Id, ProtectedIdPurpose.Patient),
                        PatientNumber = newPatient.PatientNumber,
                        AppointmentId = await protectionProvider.EncryptIntIdAsync(newAppointment.Id, ProtectedIdPurpose.Appointment),
                        AppointmentDateFrom = newAppointment.AppointmentDateFrom,
                        AppointmentDateTo = newAppointment.AppointmentDateTo,
                    });
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
