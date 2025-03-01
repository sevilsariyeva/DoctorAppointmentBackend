using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
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


    }

}
