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
        [HttpPost("add-doctor")]
        public async Task<IActionResult> AddDoctor([FromForm] DoctorDto doctorDto, [FromForm] IFormFile image)
        {
            await _adminService.AddDoctorAsync(doctorDto, image);
            return Ok(new { success = true, message = "Doctor added successfully" });
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var response = await _appointmentService.GetUserAppointmentsAsync();

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

        [HttpDelete("cancel-appointment/{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(string appointmentId)
        {
            try
            {
                var userId = await _appointmentService.GetUserIdByAppointmentIdAsync(appointmentId);
                if (string.IsNullOrEmpty(userId))
                {
                    return NotFound(new { success = false, message = "User not found for the given appointment." });
                }

                var result = await _appointmentService.CancelAppointmentAsync(userId, appointmentId);
                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Appointment cancelled successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal Server Error: {ex.Message}" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var doctorsCount = await _adminService.GetDoctorsCountAsync();
                var appointmentsCount = await _adminService.GetAppointmentsCountAsync();
                var patientsCount = await _adminService.GetPatientsCountAsync();
                var latestAppointments = await _adminService.GetLatestAppointmentsAsync(5);

                var response = new
                {
                    success = true,
                    dashData = new
                    {
                        doctors = doctorsCount,
                        appointments = appointmentsCount,
                        patients = patientsCount,
                        latestAppointments = latestAppointments
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal Server Error: {ex.Message}" });
            }
        }

    }


}
