using EmployeeClockinSystem.Models;

namespace EmployeeClockinSystem.Interfaces
{
    public interface IDashboardRepository
    {
        Task<Employee> GetByUserId(string userId);
        Record GetLastClockInClockRecord(Employee employee);


    }
}