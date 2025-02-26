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
        [HttpPut("change-availability/{doctorId}")]
        public async Task<IActionResult> ChangeAvailability(ChangeAvailabilityRequest request)
        {
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
            try
            {
                var token = await _doctorService.LoginDoctor(request.Email, request.Password);
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

    }

}
