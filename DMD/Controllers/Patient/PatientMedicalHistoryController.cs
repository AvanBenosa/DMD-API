using DMD.APPLICATION.PatientsModule.PatientMedicalHistory.Model;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using Commands = DMD.APPLICATION.PatientsModule.PatientMedicalHistory.Commands;
using Queries = DMD.APPLICATION.PatientsModule.PatientMedicalHistory.Queries;

namespace DMD.API.Controllers.Patient
{
    [Route("api/dmd/patient-medical-history")]
    public class PatientMedicalHistoryController : BaseController
    {
        [HttpGet("get-patient-medical-history")]
        [Description("Query return Patient Medical History model")]
        [ProducesResponseType(typeof(List<PatientMedicalHistoryModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMedicalHistory([FromQuery] Queries.GetByParams.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<List<PatientMedicalHistoryModel>>)result).Data;
            return Ok(data);
        }

        [HttpPost("create-patient-medical-history")]
        [Description("Create Patient based on json body")]
        [ProducesResponseType(typeof(PatientMedicalHistoryModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateMedicalHistory([FromBody] Commands.Create.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);
            var data = ((SuccessResponse<PatientMedicalHistoryModel>)result).Data;
            return Created("", data);
        }

        [HttpPut("put-patient-medical-history")]
        [Description("Update Patient Info based on param ID and json data")]
        [ProducesResponseType(typeof(PatientMedicalHistoryModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateMedicalHistory([FromBody] Commands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PatientMedicalHistoryModel>)result).Data;
            return Ok(data);
        }

        [HttpDelete("delete-patient-medical-history")]
        [Description("Delete Patient Info, returns boolean")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteMedicalHistory([FromBody] Commands.Delete.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<bool>)result).Data;
            return Ok(data);
        }
    }
}
