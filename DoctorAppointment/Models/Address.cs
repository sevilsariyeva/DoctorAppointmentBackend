using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DoctorAppointment.Models
{
    public class Address
    {
        [BsonElement("line1")]
        public string Line1 { get; set; }

        [BsonElement("line2")]
        public string Line2 { get; set; }
    }
}
