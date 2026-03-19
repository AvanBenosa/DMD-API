using DMD.APPLICATION.PublicRegistration.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using Commands = DMD.APPLICATION.PublicRegistration.Commands;
using Queries = DMD.APPLICATION.PublicRegistration.Queries;

namespace DMD.API.Controllers.Public
{
    [Route("api/public/registration")]
    public class PublicRegistrationController : BaseController
    {
        [AllowAnonymous]
        [HttpPost("request-email-verification-code")]
        [Description("Send a verification code to a public registration email address")]
        [ProducesResponseType(typeof(PublicEmailVerificationCodeResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RequestEmailVerificationCode(
            [FromBody] Commands.RequestEmailVerificationCode.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PublicEmailVerificationCodeResponseModel>)result).Data;
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("verify-email-verification-code")]
        [Description("Verify a public registration email verification code")]
        [ProducesResponseType(typeof(PublicEmailVerificationStatusModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> VerifyEmailVerificationCode(
            [FromBody] Commands.VerifyEmailVerificationCode.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PublicEmailVerificationStatusModel>)result).Data;
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("clinic")]
        [Description("Get public clinic registration context from QR clinic id")]
        [ProducesResponseType(typeof(PublicClinicRegistrationContextModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetClinic([FromQuery] Queries.GetClinic.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PublicClinicRegistrationContextModel>)result).Data;
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("existing-patient")]
        [Description("Find an existing patient record for public appointment registration")]
        [ProducesResponseType(typeof(PublicExistingPatientLookupModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FindExistingPatient([FromQuery] Queries.FindExistingPatient.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PublicExistingPatientLookupModel>)result).Data;
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("create-patient-appointment")]
        [Description("Create patient info and appointment request using public clinic QR context")]
        [ProducesResponseType(typeof(PublicPatientAppointmentRegistrationModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreatePatientAppointment(
            [FromBody] Commands.CreatePatientAppointment.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<PublicPatientAppointmentRegistrationModel>)result).Data;
            return Created(string.Empty, data);
        }
    }
}
