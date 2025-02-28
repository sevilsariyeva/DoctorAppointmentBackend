using DoctorAppointment.Models;
using DoctorAppointment.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DoctorAppointment.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Appointment> _appointmentsCollection;
        public UserRepository(MongoDbService mongoDbService)
        {
            _usersCollection = mongoDbService.GetUsersCollection();
            _appointmentsCollection = mongoDbService.GetAppointmentsCollection();
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                await _usersCollection.InsertOneAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _usersCollection
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Id))
                throw new ArgumentException("Invalid user data", nameof(user));

            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
                var updateDefinition = Builders<User>.Update;

                var updates = new List<UpdateDefinition<User>>
                    {
                        updateDefinition.Set(u => u.FullName, user.FullName),
                        updateDefinition.Set(u => u.ImageUrl, user.ImageUrl),
                        updateDefinition.Set(u => u.Address, user.Address),
                        updateDefinition.Set(u => u.Gender, user.Gender),
                        updateDefinition.Set(u => u.Dob, user.Dob),
                        updateDefinition.Set(u => u.Phone, user.Phone)
                    }.Where(update => update != null).ToList();

                if (!updates.Any()) return true;

                var update = updateDefinition.Combine(updates);
                var result = await _usersCollection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
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
        public async Task<Appointment> GetAppointmentByIdAsync(string appointmentId)
        {
            return await _appointmentsCollection
                .Find(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();
        }

    }
}
