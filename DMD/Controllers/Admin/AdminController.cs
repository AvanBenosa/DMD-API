using DMD.APPLICATION.AdminPortal.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using AdminQueries = DMD.APPLICATION.AdminPortal.Queries;
using AdminCommands = DMD.APPLICATION.AdminPortal.Commands;

namespace DMD.API.Controllers.Admin
{
    [Route("api/dmd/admin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AdminController : BaseController
    {
        [HttpGet("dashboard-summary")]
        [Description("Get admin portal dashboard summary")]
        [ProducesResponseType(typeof(AdminDashboardModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var result = await Mediator.Send(new AdminQueries.GetDashboard.Query());
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<AdminDashboardModel>)result).Data;
            return Ok(data);
        }

        [HttpGet("get-clinics")]
        [Description("Get admin portal clinics")]
        [ProducesResponseType(typeof(List<AdminClinicModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetClinics()
        {
            var result = await Mediator.Send(new AdminQueries.GetClinics.Query());
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<List<AdminClinicModel>>)result).Data;
            return Ok(data);
        }

        [HttpPost("set-clinic-lock")]
        [Description("Update clinic lock status")]
        [ProducesResponseType(typeof(AdminClinicModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SetClinicLock([FromBody] AdminCommands.UpdateClinicLock.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
            {
                return BadRequest(result.Message);
            }

            var data = ((SuccessResponse<AdminClinicModel>)result).Data;
            return Ok(data);
        }
    }
}
