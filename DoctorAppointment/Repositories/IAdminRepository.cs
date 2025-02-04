using DoctorAppointment.Models;

namespace DoctorAppointment.Repositories
{
    public interface IAdminRepository
    {
        Admin GetAdminByEmail(string email);
    }
}
