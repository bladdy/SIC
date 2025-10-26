using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserCreditsController : ControllerBase
    {
        private readonly IUserCreditUnitsOfWork _creditUnitsOfWork;

        public UserCreditsController(IUserCreditUnitsOfWork creditUnitsOfWork)
        {
            _creditUnitsOfWork = creditUnitsOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var response = await _creditUnitsOfWork.GetPlannersWithCreditsAsync();
            return response.Success ? Ok(response.Result) : BadRequest(response.Message);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUserAsync(string userId)
        {
            var response = await _creditUnitsOfWork.GetByUserIdAsync(userId);
            return response.Success ? Ok(response.Result) : NotFound(response.Message);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCreditsAsync(AddCreditsRequest request)
        {
            var response = await _creditUnitsOfWork.AddAsync(request);
            return response.Success ? Ok(response.Result) : BadRequest(response.Message);
        }

        [HttpPost("consume/{userId}")]
        public async Task<IActionResult> ConsumeCreditAsync(string userId)
        {
            var response = await _creditUnitsOfWork.ConsumeCreditAsync(userId);
            return response.Success ? Ok(response.Result) : BadRequest(response.Message);
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistoryAsync(string userId)
        {
            var response = await _creditUnitsOfWork.GetHistoryAsync(userId);
            return response.Success ? Ok(response.Result) : NotFound(response.Message);
        }
    }
}