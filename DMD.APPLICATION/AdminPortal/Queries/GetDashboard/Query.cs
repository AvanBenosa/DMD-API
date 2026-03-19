using DMD.APPLICATION.AdminPortal.Models;
using DMD.APPLICATION.Auth;
using DMD.APPLICATION.Common.ProtectedIds;
using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using DMD.SERVICES.ProtectionProvider;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NJsonSchema.Annotations;
using System.Security.Claims;
using DMD.DOMAIN.Enums;

namespace DMD.APPLICATION.AdminPortal.Queries.GetDashboard
{
    [JsonSchema("GetAdminDashboardQuery")]
    public class Query : IRequest<Response>
    {
    }

    public class QueryHandler : IRequestHandler<Query, Response>
    {
        private readonly DmdDbContext dbContext;
        private readonly IProtectionProvider protectionProvider;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public QueryHandler(
            DmdDbContext dbContext,
            IProtectionProvider protectionProvider,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.protectionProvider = protectionProvider;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(currentUserId))
                {
                    return new BadRequestResponse("Unauthorized access.");
                }

                var currentUser = await dbContext.UserProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken);

                if (currentUser == null || !AuthResponseFactory.IsBootstrapSeedUser(currentUser, configuration))
                {
                    return new BadRequestResponse("Admin portal access is restricted.");
                }
                var clinics = await dbContext.ClinicProfiles
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .OrderBy(x => x.ClinicName)
                    .Select(x => new
                    {
                        x.Id,
                        x.ClinicName,
                        x.IsLocked,
                        x.IsDataPrivacyAccepted,
                        DoctorCount = dbContext.UserProfiles.Count(user =>
                            user.ClinicId == x.Id && user.Role == UserRole.Dentist),
                        PatientCount = dbContext.PatientInfos.Count(patient =>
                            patient.ClinicProfileId == x.Id)
                    })
                    .ToListAsync(cancellationToken);

                var today = DateTime.Today;
                var trendStartDate = today.AddDays(-6);
                var patientTrendRows = await dbContext.PatientInfos
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(patient => patient.CreatedAt >= trendStartDate)
                    .GroupBy(patient => new { patient.ClinicProfileId, CreatedDate = patient.CreatedAt.Date })
                    .Select(group => new
                    {
                        group.Key.ClinicProfileId,
                        group.Key.CreatedDate,
                        PatientCount = group.Count()
                    })
                    .ToListAsync(cancellationToken);

                var owners = await dbContext.UserProfiles
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(user => user.Role == UserRole.SuperAdmin && user.ClinicId.HasValue)
                    .Select(user => new
                    {
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.EmailAddress,
                        user.ContactNumber,
                        user.ClinicId,
                        ClinicName = dbContext.ClinicProfiles
                            .IgnoreQueryFilters()
                            .Where(clinic => clinic.Id == user.ClinicId)
                            .Select(clinic => clinic.ClinicName)
                            .FirstOrDefault()
                    })
                    .ToListAsync(cancellationToken);

                var response = new AdminDashboardModel
                {
                    TotalClinics = clinics.Count,
                    TotalDoctors = clinics.Sum(x => x.DoctorCount),
                    TotalPatients = clinics.Sum(x => x.PatientCount),
                    Clinics = (await Task.WhenAll(clinics.Select(async x => new AdminClinicDashboardItemModel
                    {
                        Id = await protectionProvider.EncryptIntIdAsync(x.Id, ProtectedIdPurpose.Clinic) ?? string.Empty,
                        ClinicName = x.ClinicName,
                        DoctorCount = x.DoctorCount,
                        PatientCount = x.PatientCount,
                        IsLocked = x.IsLocked,
                        IsDataPrivacyAccepted = x.IsDataPrivacyAccepted,
                    }))).ToList(),
                    DailyPatientTrends = Enumerable.Range(0, 7)
                        .Select(offset =>
                        {
                            var currentDate = trendStartDate.AddDays(offset);
                            return new AdminDashboardPatientTrendModel
                            {
                                Date = currentDate.ToString("yyyy-MM-dd"),
                                Label = currentDate.ToString("MMM dd"),
                                Clinics = clinics
                                    .Select(clinic => new AdminDashboardPatientTrendClinicModel
                                    {
                                        ClinicName = clinic.ClinicName,
                                        PatientCount = patientTrendRows
                                            .FirstOrDefault(item =>
                                                item.ClinicProfileId == clinic.Id
                                                && item.CreatedDate == currentDate)?.PatientCount ?? 0
                                    })
                                    .ToList()
                            };
                        })
                        .ToList(),
                    Owners = owners
                        .Where(owner => !AuthResponseFactory.IsBootstrapSeedUser(new DMD.DOMAIN.Entities.UserProfile.UserProfile
                        {
                            Role = UserRole.SuperAdmin,
                            ClinicId = owner.ClinicId,
                            Email = owner.Email,
                            EmailAddress = owner.EmailAddress,
                            UserName = owner.Email ?? owner.EmailAddress ?? string.Empty
                        }, configuration))
                        .OrderBy(owner => owner.ClinicName)
                        .ThenBy(owner => owner.LastName)
                        .ThenBy(owner => owner.FirstName)
                        .Select(owner => new AdminDashboardOwnerModel
                        {
                            Name = string.Join(" ", new[] { owner.FirstName, owner.LastName }
                                .Where(value => !string.IsNullOrWhiteSpace(value))),
                            ClinicName = owner.ClinicName ?? string.Empty,
                            EmailAddress = owner.Email ?? owner.EmailAddress ?? string.Empty,
                            ContactNumber = owner.ContactNumber ?? string.Empty
                        })
                        .ToList()
                };

                return new SuccessResponse<AdminDashboardModel>(response);
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
        }
    }
}
