using DMD.APPLICATION.PatientsModule.Patient.Models;
using DMD.APPLICATION.PatientsModule.PatientProfile.Model;
using DMD.APPLICATION.Responses;
using DMD.API.Storage;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using System.Security.Claims;
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
        private readonly IClinicStorageService clinicStorageService;

        public PatientController(IClinicStorageService clinicStorageService)
        {
            this.clinicStorageService = clinicStorageService;
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

            var clinicIdValue = User.FindFirstValue("clinicId");
            if (!int.TryParse(clinicIdValue, out var clinicId))
            {
                return BadRequest("Authenticated clinic was not found.");
            }

            var storedFile = await clinicStorageService.SaveClinicFileAsync(
                clinicId,
                file,
                HttpContext.RequestAborted,
                "patients",
                "profile-pictures");

            clinicStorageService.DeleteClinicFileIfOwned(
                request?.OldFilePath,
                clinicId,
                "patients",
                "profile-pictures");

            return Ok(new ProfilePictureUploadModel
            {
                FileName = storedFile.FileName,
                FilePath = storedFile.FilePath
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

    }
}
