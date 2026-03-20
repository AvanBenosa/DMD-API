using DMD.APPLICATION.BuildUps.TemplateForm.Models;
using DMD.APPLICATION.Responses;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;
using Commands = DMD.APPLICATION.BuildUps.TemplateForm.Commands;
using Queries = DMD.APPLICATION.BuildUps.TemplateForm.Queries;

namespace DMD.API.Controllers.BuildUps
{
    [Route("api/dmd/template-form")]
    public class TemplateFormController : BaseController
    {
        [HttpGet("get-template-form")]
        [Description("Query returns template forms for the authenticated clinic")]
        [ProducesResponseType(typeof(TemplateFormResponseModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTemplateForm([FromQuery] Queries.GetByParams.Query query)
        {
            var result = await Mediator.Send(query);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<TemplateFormResponseModel>)result).Data;
            return Ok(data);
        }

        [HttpPost("create-template-form")]
        [Description("Create template form based on json body")]
        [ProducesResponseType(typeof(TemplateFormModel), (int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreateTemplateForm([FromBody] Commands.Create.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<TemplateFormModel>)result).Data;
            return Created("", data);
        }

        [HttpPut("put-template-form")]
        [Description("Update template form based on json body")]
        [ProducesResponseType(typeof(TemplateFormModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateTemplateForm([FromBody] Commands.Update.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<TemplateFormModel>)result).Data;
            return Ok(data);
        }

        [HttpDelete("delete-template-form")]
        [Description("Delete template form, returns deleted id")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteTemplateForm([FromBody] Commands.Delete.Command command)
        {
            var result = await Mediator.Send(command);
            if (result is BadRequestResponse)
                return BadRequest(result.Message);

            var data = ((SuccessResponse<string>)result).Data;
            return Ok(data);
        }
    }
}
