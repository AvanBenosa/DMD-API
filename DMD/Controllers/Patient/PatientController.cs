using DMD.APPLICATION.PatientsModule.Patient.Models;
using DMD.APPLICATION.PatientsModule.PatientProfile.Model;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using Commands = DMD.APPLICATION.PatientsModule.Patient.Commands;
using Queries = DMD.APPLICATION.PatientsModule.Patient.Queries;


namespace DMD.API.Controllers.Patient
{
    public class UploadProfilePictureRequest
    {
        public IFormFile? File { get; set; }
        public string? OldFilePath { get; set; }
    }

    public class UploadPatientExcelRequest
    {
        public IFormFile? File { get; set; }
    }

    [Route("api/dmd/patient")]
    public class PatientController : BaseController
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public PatientController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("get-patient")]
        [Description("Query return Patient info model")]
        [ProducesResponseType(typeof(PatientResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPatient([FromQuery] Queries.GetByParams.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PatientResponseModel>)result).Data;
            return Ok(data);
        }

        [HttpPost("create-patient")]
        [Description("Create Patient based on json body")]
        [ProducesResponseType(typeof(PatientModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreatePatient([FromBody] Commands.Create.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);
            var data = ((SuccessResponse<PatientModel>)result).Data;
            return Created("", data);
        }

        [HttpPut("put-patient")]
        [Description("Update Patient Info based on param ID and json data")]
        [ProducesResponseType(typeof(PatientModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdatePatient([FromBody] Commands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PatientModel>)result).Data;
            return Ok(data);
        }

        [HttpDelete("delete-patient")]
        [Description("Delete Patient Info, returns boolean")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeletePatient([FromBody] Commands.Delete.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<bool>)result).Data;
            return Ok(data);
        }

        [HttpPost("upload-profile-picture")]
        [Description("Upload patient profile picture and return saved file path")]
        [ProducesResponseType(typeof(ProfilePictureUploadModel), (int)HttpStatusCode.OK)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadProfilePicture([FromForm] UploadProfilePictureRequest request)
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
                "patients");

            Directory.CreateDirectory(uploadsFolder);

            var generatedFileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsFolder, generatedFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            DeleteOldProfilePictureIfOwned(request?.OldFilePath, uploadsFolder);

            return Ok(new ProfilePictureUploadModel
            {
                FileName = generatedFileName,
                FilePath = $"/uploads/patients/{generatedFileName}"
            });
        }

        [HttpPost("upload-patient-xlsx")]
        [Description("Upload patient xlsx file and import patient records")]
        [ProducesResponseType(typeof(PatientUploadResultModel), (int)HttpStatusCode.OK)]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> UploadPatientXlsx([FromForm] UploadPatientExcelRequest request)
        {
            var file = request?.File;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var command = new Commands.Upload.Command
            {
                FileName = file.FileName,
                FileContent = memoryStream.ToArray(),
            };

            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PatientUploadResultModel>)result).Data;
            return Ok(data);
        }

        private static void DeleteOldProfilePictureIfOwned(string? oldFilePath, string uploadsFolder)
        {
            if (string.IsNullOrWhiteSpace(oldFilePath))
                return;

            var normalizedPath = oldFilePath.Replace('\\', '/').Trim();

            if (!normalizedPath.StartsWith("/uploads/patients/", StringComparison.OrdinalIgnoreCase))
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
