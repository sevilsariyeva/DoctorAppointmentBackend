using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models.Dtos
{
    public class DoctorDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Experience { get; set; }
        public decimal? Fees { get; set; }

        public string? About { get; set; }
        public bool? Available { get; set; }

        public string? Speciality { get; set; }

        public string? Degree { get; set; }

        public string? Address1 { get; set; }
        public string? Address2 { get; set; }

        public string? Image { get; set; }
    }

}
