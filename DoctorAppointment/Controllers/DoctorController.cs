using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Controllers
{
    [ApiController]
    [Route("api/doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
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
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { success = false, message = "Email and password are required." });
            }

            var token = await _doctorService.LoginDoctor(request);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Invalid credentials" });
            }

            return Ok(new { success = true, token });
        }

    }

}
