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
    [Route("api/user")]
    public class UserController:ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;

        public UserController(IUserService userService,IAppointmentService appointmentService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _userService.RegisterUserAsync(request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { success = false, message = "Email and password are required." });
            }

            var token = await _userService.LoginUserAsync(request);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { success = false, message = "Invalid credentials" });
            }

            return Ok(new { success = true, token });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized("Invalid token.");
                }

                var response = await _userService.GetProfileAsync(currentUserId);
                if (!response.Success)
                {
                    return BadRequest(response.Message);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            try
            {
                var result = await _userService.UpdateUserAsync(currentUserId, request);
                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new { success = true, message = "Profile updated successfully.", data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("book-appointment")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentRequest request)
        {
            var response = await _appointmentService.BookAppointmentAsync(request.UserId, request.DocId, request.SlotDate, request.SlotTime);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
        [Authorize]
        [HttpGet("appointments")]
        public async Task<IActionResult> GetUserAppointments()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid token.");
                }

                var response = await _appointmentService.GetUserAppointmentsAsync(userId);
                if (!response.Success)
                {
                    return BadRequest(response.Message);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("cancel-appointment/{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(CancelAppointmentRequest request)
        {
            request.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(request.UserId))
            {
                return Unauthorized(new { success = false, message = "Invalid token." });
            }

            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(request, false);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "Failed to cancel the appointment. Please try again later." });
                }

                return Ok(new { success = true, message = "Appointment canceled successfully." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest paymentRequest)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(paymentRequest.AppointmentId);
                if (appointment == null)
                {
                    return NotFound(new { success = false, message = "Appointment not found." });
                }

                var doctor = await _appointmentService.GetDoctorByAppointmentIdAsync(paymentRequest.AppointmentId);
                if (doctor?.Fees == null)
                {
                    return BadRequest(new { success = false, message = "Doctor's fees are not available." });
                }

                paymentRequest.Payment = doctor.Fees;

                var paymentSuccess = true; 

                if (!paymentSuccess)
                {
                    return BadRequest(new { success = false, message = "Payment failed. Please try again." });
                }

                appointment.Payment = paymentRequest.Payment;
                await _appointmentService.UpdateAppointmentAsync(appointment);

                return Ok(new { success = true, message = "Payment successful!" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

    }
}
