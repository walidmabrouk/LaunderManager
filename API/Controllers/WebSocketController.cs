using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/machines")]
    public class WebSocketController : ControllerBase
    {
        private readonly IMachineService _machineService;
        private readonly WebSocketService _connectionManager;

        public WebSocketController(IMachineService machineService)
        {
            _machineService = machineService;
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartMachine(int id)
        {
            await _machineService.HandleMachineStartAsync(id);
            return Ok("Machine started.");
        }
        
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopMachine(int id, [FromBody] decimal cyclePrice)
        {
            await _machineService.HandleMachineStopAsync(id, cyclePrice);
            return Ok("Machine stopped and cycle price updated.");
        }
    }
}
