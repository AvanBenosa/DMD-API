using DMD.APPLICATION.Auth.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using LoginAuthResponse = DMD.APPLICATION.Auth.Models.AuthResponse;
using RegistrationStatusResponse = DMD.APPLICATION.Auth.Models.RegistrationStatusResponse;
using VerificationCodeResponse = DMD.APPLICATION.Auth.Models.VerificationCodeResponse;

using Commands = DMD.APPLICATION.Auth.Commands;
using RegisterCommands = DMD.APPLICATION.Auth.Commands.Register;
using RequestVerificationCodeCommands = DMD.APPLICATION.Auth.Commands.RequestClinicRegistrationCode;
using RegisterClinicCommands = DMD.APPLICATION.Auth.Commands.RegisterClinic;
using RegistrationStatusQueries = DMD.APPLICATION.Auth.Queries.GetRegistrationStatus;

namespace DMD.API.Controllers.Auth
{
    public class AuthController : BaseController
    {
        [AllowAnonymous]
        [HttpPost("login")]
        [Description("Login")]
        [ProducesResponseType(typeof(LoginAuthResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> LoginUser([FromBody] Commands.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<LoginAuthResponse>)result).Data;
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("register/clinic/request-code")]
        [HttpPost("api/register/request-code")]
        [Description("Request a verification code for public clinic registration")]
        [ProducesResponseType(typeof(VerificationCodeResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RequestClinicRegistrationCode(
            [FromBody] RequestVerificationCodeCommands.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<VerificationCodeResponse>)result).Data;
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("register/clinic")]
        [HttpPost("api/register/clinic")]
        [Description("Register a clinic and its super admin using a verification code")]
        [ProducesResponseType(typeof(LoginAuthResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RegisterClinic([FromBody] RegisterClinicCommands.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<LoginAuthResponse>)result).Data;
            return Ok(data);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("register/bootstrap")]
        [HttpPost("api/register/create")]
        [Description("Complete bootstrap registration")]
        [ProducesResponseType(typeof(LoginAuthResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RegisterBootstrapUser([FromBody] RegisterCommands.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<LoginAuthResponse>)result).Data;
            return Ok(data);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("register/status")]
        [HttpGet("api/register/status")]
        [Description("Get bootstrap registration status")]
        [ProducesResponseType(typeof(RegistrationStatusResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRegistrationStatus()
        {
            var result = await Mediator.Send(new RegistrationStatusQueries.Query());
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<RegistrationStatusResponse>)result).Data;
            return Ok(data);
        }
    }
}
