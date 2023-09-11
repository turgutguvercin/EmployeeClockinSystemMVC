using EmployeeClockinSystem.Models;

namespace EmployeeClockinSystem.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee> GetCurrentEmployee(string curUserId);
        Task<List<Employee>> GetAllEmployees();
        Task<ICollection<Schedule>> GetSchedulesByStartDate(DateTime startDate);
        Record GetLastClockInClockRecord(Employee employee);
        bool DeleteSchedulesRange(DateTime startDate);
        bool UpdateRecord(Record recordToUpdate);
        bool AddSchedule(Schedule schedule);
        bool AddRecord(Record record);
        bool Delete(Record record);
        bool Save();
    }
}