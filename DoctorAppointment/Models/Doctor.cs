using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models
{
    public class Doctor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Required]
        [BsonElement("name")]
        public string Name { get; set; }

        [Required]
        [BsonElement("email")]
        public string Email { get; set; }

        [Required]
        [BsonElement("password")]
        public string Password { get; set; }

        [Required]
        [BsonElement("experience")]
        public string Experience { get; set; }

        [Required]
        [BsonElement("fees")]
        public decimal Fees { get; set; }

        [Required]
        [BsonElement("about")]
        public string About { get; set; }

        [Required]
        [BsonElement("speciality")]
        public string Speciality { get; set; }

        [Required]
        [BsonElement("degree")]
        public string Degree { get; set; }

        [BsonElement("imagePath")]
        public string ImagePath { get; set; } // Şəkilin yolu

        [BsonElement("address")]
        public Address Address { get; set; }
    }

    
}
