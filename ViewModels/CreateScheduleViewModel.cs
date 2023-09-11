
namespace EmployeeClockinSystem.ViewModels
{
    public class CreateScheduleViewModel
    {
        public Dictionary<string, ScheduleViewModel> ScheduleLookup { get; set; }
        public ICollection<ScheduleViewModel> PostedSchedules { get; set; }
        public DateTime StartDateOfWeek { get; set; }
    }
}