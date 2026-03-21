using DMD.APPLICATION.Finances.ClinicExpenses.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using Commands = DMD.APPLICATION.Finances.ClinicExpenses.Commands;
using Queries = DMD.APPLICATION.Finances.ClinicExpenses.Queries;

namespace DMD.API.Controllers.Finance
{
    [Route("api/dmd/clinic-expenses")]
    public class ClinicExpensesController : BaseController
    {
        [HttpGet("get-clinic-expenses")]
        [Description("Query returns clinic expense records for the authenticated clinic")]
        [ProducesResponseType(typeof(List<ClinicExpensesModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetClinicExpenses([FromQuery] Queries.GetByParams.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<List<ClinicExpensesModel>>)result).Data;
            return Ok(data);
        }

        [HttpPost("create-clinic-expenses")]
        [Description("Create clinic expense based on json body")]
        [ProducesResponseType(typeof(ClinicExpensesModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateClinicExpenses([FromBody] Commands.Create.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<ClinicExpensesModel>)result).Data;
            return Created("", data);
        }

        [HttpPut("put-clinic-expenses")]
        [Description("Update clinic expense based on json body")]
        [ProducesResponseType(typeof(ClinicExpensesModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateClinicExpenses([FromBody] Commands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<ClinicExpensesModel>)result).Data;
            return Ok(data);
        }

        [HttpDelete("delete-clinic-expenses")]
        [Description("Delete clinic expense, returns boolean")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteClinicExpenses([FromBody] Commands.Delete.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<bool>)result).Data;
            return Ok(data);
        }
    }
}
