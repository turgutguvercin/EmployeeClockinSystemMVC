
using EmployeeClockinSystem.Models;

namespace EmployeeClockinSystem.ViewModels
{
    public class EmployeeWeeklyViewModel
    {
        public string EmployeeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Record> Records { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}