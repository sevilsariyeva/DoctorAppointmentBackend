using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController:ControllerBase
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _adminService.LoginAdmin(request.Email, request.Password);
                return Ok(new { success = true, token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { success = false, message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var response = await _adminService.GetUserAppointmentsAsync();

                if (!response.Success)
                {
                    return BadRequest(new ServiceResponse<List<Appointment>>(null, response.Message, false));
                }

                return Ok(new ServiceResponse<List<Appointment>>(response.Data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<List<Appointment>>(null, $"Internal Server Error: {ex.Message}", false));
            }
        }



    }


}
