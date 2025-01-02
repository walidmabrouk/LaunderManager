using LaunderWebApi.Entities;
using Laundromat.Application.UseCases;
using Laundromat.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace LaunderManagerWebApi.Presentation.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProprietorController : ControllerBase
    {

        private readonly UploadInitialConfigurationUseCase _useCase;

        public ProprietorController(UploadInitialConfigurationUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpPost("upload-configuration")]
        public async Task<IActionResult> UploadConfiguration([FromBody] Proprietor configurationJson)
        {
            try
            {
                await _useCase.ExecuteAsync(configurationJson);
                return Ok("Configuration uploaded successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
