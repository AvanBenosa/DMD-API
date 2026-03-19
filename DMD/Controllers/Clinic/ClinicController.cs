using DMD.APPLICATION.ClinicProfiles.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using DMD.APPLICATION.PatientsModule.Patient.Models;

using ClinicCommands = DMD.APPLICATION.ClinicProfiles.Commands;
using ClinicModels = DMD.APPLICATION.ClinicProfiles.Models;
using ClinicQueries = DMD.APPLICATION.ClinicProfiles.Queries;

namespace DMD.API.Controllers.Clinic
{
    public class UploadClinicBannerRequest
    {
        public IFormFile? File { get; set; }
        public string? OldFilePath { get; set; }
    }

    [Route("api/dmd/clinic")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClinicController : BaseController
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public ClinicController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("create-clinic")]
        [Description("Create clinic profile")]
        [ProducesResponseType(typeof(ClinicProfileModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateClinic([FromBody] ClinicCommands.Create.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<ClinicProfileModel>)result).Data;
            return Created("", data);
        }

        [HttpGet("get-current-clinic-profile")]
        [Description("Get current clinic profile")]
        [ProducesResponseType(typeof(ClinicProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCurrentClinicProfile([FromQuery] ClinicQueries.GetCurrent.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<ClinicProfileModel>)result).Data;
            return Ok(data);
        }

        [HttpPost("upload-banner")]
        [Description("Upload clinic banner and return saved file path")]
        [ProducesResponseType(typeof(ProfilePictureUploadModel), (int)HttpStatusCode.OK)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadBanner([FromForm] UploadClinicBannerRequest request)
        {
            var file = request?.File;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type. Allowed: jpg, jpeg, png, gif, webp.");

            var uploadsFolder = Path.Combine(
                webHostEnvironment.WebRootPath ?? Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot"),
                "uploads",
                "clinics");

            Directory.CreateDirectory(uploadsFolder);

            var generatedFileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsFolder, generatedFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            DeleteOldBannerIfOwned(request?.OldFilePath, uploadsFolder);

            return Ok(new ProfilePictureUploadModel
            {
                FileName = generatedFileName,
                FilePath = $"/uploads/clinics/{generatedFileName}"
            });
        }

        [HttpPut("put-clinic-profile")]
        [Description("Update clinic profile")]
        [ProducesResponseType(typeof(ClinicProfileModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateClinicProfile([FromBody] ClinicCommands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<ClinicProfileModel>)result).Data;
            return Ok(data);
        }

        [HttpGet("data-privacy-status")]
        [Description("Get current clinic data privacy acceptance status")]
        [ProducesResponseType(typeof(ClinicModels.DataPrivacyStatusModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDataPrivacyStatus()
        {
            var result = await Mediator.Send(new ClinicQueries.GetDataPrivacyStatus.Query());
            if (result is NotFoundResponse)
            {
                return NotFound(result.Message);
            }

            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<ClinicModels.DataPrivacyStatusModel>)result).Data;
            return Ok(data);
        }

        [HttpPost("accept-data-privacy")]
        [Description("Accept data privacy for the current clinic")]
        [ProducesResponseType(typeof(ClinicModels.DataPrivacyStatusModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AcceptDataPrivacy()
        {
            var result = await Mediator.Send(new ClinicCommands.AcceptDataPrivacy.Command());
            if (result is NotFoundResponse)
            {
                return NotFound(result.Message);
            }

            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<ClinicModels.DataPrivacyStatusModel>)result).Data;
            return Ok(data);
        }

        private static void DeleteOldBannerIfOwned(string? oldFilePath, string uploadsFolder)
        {
            if (string.IsNullOrWhiteSpace(oldFilePath))
                return;

            var normalizedPath = oldFilePath.Replace('\\', '/').Trim();

            if (!normalizedPath.StartsWith("/uploads/clinics/", StringComparison.OrdinalIgnoreCase))
                return;

            var oldFileName = Path.GetFileName(normalizedPath);

            if (string.IsNullOrWhiteSpace(oldFileName))
                return;

            var existingPhysicalPath = Path.Combine(uploadsFolder, oldFileName);

            if (System.IO.File.Exists(existingPhysicalPath))
                System.IO.File.Delete(existingPhysicalPath);
        }
    }
}
