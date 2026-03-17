using DMD.APPLICATION.Dashboard.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using Queries = DMD.APPLICATION.Dashboard.Queries;

namespace DMD.API.Controllers.Dashboard
{
    [Route("api/dmd/dashboard")]
    public class DashboardController : BaseController
    {
        [HttpGet("get")]
        [HttpGet("get-dashboard")]
        [Description("Query return Dashboard Response model")]
        [ProducesResponseType(typeof(DashboardResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FetchDashboard([FromQuery] Queries.GetByParams.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<DashboardResponseModel>)result).Data;
            return Ok(data);
        }
    }
}
