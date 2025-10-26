using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using System.Security.Claims;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardUnitOfWork _dashboardUnitOfWork;

        public DashboardController(IDashboardUnitOfWork dashboardUnitOfWork)
        {
            _dashboardUnitOfWork = dashboardUnitOfWork;
        }

        [HttpGet("admin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var action = await _dashboardUnitOfWork.GetAdminDashboardAsync(userId);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return BadRequest();
        }

        [HttpGet("planner")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "WeddingPlanner")]
        public async Task<IActionResult> Planner()
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized("No se pudo obtener el ID del usuario autenticado.");

            var action = await _dashboardUnitOfWork.GetPlannerDashboardAsync(userId);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return BadRequest();
        }

        [HttpGet("user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Users()
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized("No se pudo obtener el ID del usuario autenticado.");

            var action = await _dashboardUnitOfWork.GetUserDashboardAsync(userId);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return BadRequest();
        }
    }
}