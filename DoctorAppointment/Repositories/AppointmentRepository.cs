using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DoctorAppointment.Repositories
{
    public class AppointmentRepository:IAppointmentRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Appointment> _appointmentsCollection;
        private readonly IDoctorRepository _doctorRepository;
        public AppointmentRepository(MongoDbService mongoDbService, IDoctorRepository doctorRepository)
        {
            _usersCollection = mongoDbService.GetUsersCollection();
            _appointmentsCollection = mongoDbService.GetAppointmentsCollection();
            _doctorRepository = doctorRepository;
        }
        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _appointmentsCollection.InsertOneAsync(appointment);
        }
        public async Task<List<Appointment>> GetUserAppointmentsAsync(string userId)
        {
            return await _appointmentsCollection
     .Find(a => a.UserId == userId)
     .SortByDescending(a => a.SlotDate)
     .ThenByDescending(a => a.SlotTime)
     .ToListAsync();

        }
        public async Task<Appointment> GetAppointmentByUserIdAsync(string appointmentId)
        {
            return await _appointmentsCollection
                .Find(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CancelAppointmentAsync(string appointmentId, string userId = null, bool isAdmin = false, IClientSessionHandle session = null)
        {

            var filter = isAdmin
         ? Builders<Appointment>.Filter.Eq(a => a.Id, appointmentId) 
         : Builders<Appointment>.Filter.And(
             Builders<Appointment>.Filter.Eq(a => a.Id, appointmentId),
             Builders<Appointment>.Filter.Eq(a => a.UserId, userId) 
         );

            var appointment = await _appointmentsCollection.Find(filter).FirstOrDefaultAsync();
            if (appointment == null)
            {
                return false;
            }

            var newPayment = CalculateUpdatedPayment(appointment, isAdmin);

            var update = Builders<Appointment>.Update
        .Set(a => a.Cancelled, true)
        .Set(a => a.Payment, newPayment);

            var result = session != null
                ? await _appointmentsCollection.UpdateOneAsync(session, filter, update)
                : await _appointmentsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        private decimal? CalculateUpdatedPayment(Appointment appointment, bool isAdmin)
        {
            if (!isAdmin || appointment.Payment == null)
                return appointment.Payment;

            var doctor = _doctorRepository.GetDoctorByIdAsync(appointment.DocId).Result;
            return doctor != null ? appointment.Payment - doctor.Fees : appointment.Payment;
        }




        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            try
            {
                var appointments = await _appointmentsCollection.Find(_ => true).ToListAsync();

                return appointments;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching appointments: {ex.Message}");
            }
        }
        public async Task<bool> UpdateAppointmentAsync(Appointment appointment)
        {
            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, appointment.Id);
            var result = await _appointmentsCollection.ReplaceOneAsync(filter, appointment);
            return result.ModifiedCount > 0;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
        {
            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, appointmentId);
            return await _appointmentsCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<Doctor?> GetDoctorByAppointmentIdAsync(string appointmentId)
        {
            var appointment = await _appointmentsCollection
                .Find(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();

            if (appointment == null || string.IsNullOrEmpty(appointment.DocId))
            {
                return null;
            }

            var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);
            return doctor;
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId)
        {
            return await _appointmentsCollection
                .Find(a => a.DocId == doctorId)
                .SortByDescending(a => a.SlotDate)
                .ThenByDescending(a => a.SlotTime)
                .ToListAsync();
        }


    }
}
