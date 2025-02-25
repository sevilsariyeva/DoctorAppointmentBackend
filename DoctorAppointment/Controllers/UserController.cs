﻿using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> LoginUser([FromBody] LoginUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.LoginUserAsync(request);
            if (!result.Success)
            {
                return Unauthorized(result.Message);
            }

            return Ok(result);
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
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized("Invalid token.");
                }

                var result = await _userService.UpdateUserAsync(currentUserId, request);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
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
        public async Task<IActionResult> CancelAppointment(string appointmentId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid token.");
                }
              
                var result = await _appointmentService.CancelAppointmentAsync(userId, appointmentId);
                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
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
                if (doctor == null || doctor.Fees == null)
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
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }



    }
}
