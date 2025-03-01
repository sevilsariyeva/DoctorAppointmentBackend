using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController:ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAppointmentService _appointmentService;
        public AdminController(IAdminService adminService,IAppointmentService appointmentService)
        {
            _adminService = adminService;
            _appointmentService = appointmentService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest request)
        {
            var response = await _adminService.LoginAdmin(request);
            if (string.IsNullOrEmpty(response.Token))
            {
                return Unauthorized(new { success = false, message = "Invalid credentials." });
            }

            return Ok(new { success = true, response.Token,response.ExpiryTime });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-doctor")]
        public async Task<IActionResult> AddDoctor([FromForm] DoctorDto doctorDto, [FromForm] IFormFile image)
        {
            await _adminService.AddDoctorAsync(doctorDto, image);
            return Ok(new { success = true, message = "Doctor added successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _appointmentService.GetUserAppointmentsAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound("No appointments found.");
            }

            return Ok(appointments);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("cancel-appointment/{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(CancelAppointmentRequest request)
        {
            var userId = await _appointmentService.GetUserIdByAppointmentIdAsync(request.AppointmentId);
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound(new { success = false, message = "User not found for the given appointment." });
            }

            var result = await _appointmentService.CancelAppointmentAsync(request, true);
            if (!result)
            {
                return BadRequest(new { success = false, message = "Failed to cancel appointment." });
            }

            return Ok(new { success = true, message = "Appointment cancelled successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
                var statistics = await _adminService.GetAdminDashboardStatisticsAsync(5);
                return Ok(statistics);
        }

    }


}
