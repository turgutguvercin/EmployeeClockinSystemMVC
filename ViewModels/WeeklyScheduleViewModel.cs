using EmployeeClockinSystem.Models;

namespace EmployeeClockinSystem.ViewModels
{
    public class WeeklyScheduleViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
    }

}