using DMD.APPLICATION.Appointment.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using Commands = DMD.APPLICATION.Appointment.Commands;
using Queries = DMD.APPLICATION.Appointment.Queries;

namespace DMD.API.Controllers.Appointment
{
    [Route("api/dmd/appointment")]
    public class AppointmentController : BaseController
    {
        [HttpGet("get-appointment")]
        [Description("Query return appointment request model")]
        [ProducesResponseType(typeof(AppointmentResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAppointment([FromQuery] Queries.GetByParams.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<AppointmentResponseModel>)result).Data;
            return Ok(data);
        }

        [HttpPost("create-appointment")]
        [Description("Create appointment based on json body")]
        [ProducesResponseType(typeof(AppointmentModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateAppointment([FromBody] Commands.Create.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<AppointmentModel>)result).Data;
            return Created("", data);
        }

        [HttpPut("put-appointment")]
        [Description("Update appointment based on json body")]
        [ProducesResponseType(typeof(AppointmentModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateAppointment([FromBody] Commands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<AppointmentModel>)result).Data;
            return Ok(data);
        }

        [HttpDelete("delete-appointment")]
        [Description("Delete appointment, returns boolean")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteAppointment([FromBody] Commands.Delete.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<bool>)result).Data;
            return Ok(data);
        }
    }
}
