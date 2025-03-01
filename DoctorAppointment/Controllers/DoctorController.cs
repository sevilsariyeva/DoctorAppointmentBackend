using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IAppointmentService _appointmentService;

        public DoctorController(IDoctorService doctorService, IAppointmentService appointmentService)
        {
            _doctorService = doctorService;
            _appointmentService = appointmentService;
        }

        [HttpGet("all-doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            return Ok(doctors);
        }
        [HttpPut("change-availability")]
        public async Task<IActionResult> ChangeAvailability([FromBody] ChangeAvailabilityRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.DoctorId))
            {
                return BadRequest(new { success = false, message = "Invalid request data" });
            }

            var success = await _doctorService.ChangeAvailabilityAsync(request.DoctorId);
            if (!success)
            {
                return NotFound(new { success = false, message = "Doctor not found" });
            }

            return Ok(new { success = true, message = "Availability updated successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginDoctor([FromBody] LoginRequest request)
        {
            var response = await _doctorService.LoginDoctor(request);
            if (string.IsNullOrEmpty(response.Token))
            {
                return Unauthorized(new { success = false, message = "Invalid credentials." });
            }

            return Ok(new { success = true, response.Token, response.ExpiryTime });
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetDoctorAppointments()
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(doctorId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
            return Ok(new { success = true, appointments });
        }

        [Authorize(Roles ="Doctor")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDoctorDashboardStats()
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(doctorId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            var statistics = await _doctorService.GetDoctorDashboardStatisticsAsync(doctorId,5);
            return Ok(new { success = true, statistics });
        }

        [Authorize(Roles = "Doctor")]
        [HttpPut("complete-appointment/{appointmentId}")]
        public async Task<IActionResult> CompleteAppointment(string appointmentId, [FromBody] bool isCompleted)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null)
            {
                return NotFound(new { success = false, message = "Appointment not found." });
            }
            var request = new UpdateAppointmentRequest
            {
                AppointmentId = appointmentId,
                IsCompleted = isCompleted
            };
            var result = await _appointmentService.UpdateAppointmentAsync(request);

            if (result == null)
            {
                return BadRequest(new { success = false, message = "Failed to mark appointment as completed." });
            }

            return Ok(new { success = true, message = "Appointment marked as completed." });
        }


        [Authorize(Roles = "Doctor")]
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

    }

}
