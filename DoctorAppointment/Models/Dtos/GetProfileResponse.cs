﻿using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models.Dtos
{
    public class GetProfileResponse
    {
        public string Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ImageUrl { get; set; }
        public object Address { get; set; }
        public string Gender { get; set; }
        public DateTime Dob { get; set; }
        public string Phone { get; set; }
    }

}
