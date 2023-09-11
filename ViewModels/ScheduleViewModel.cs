namespace EmployeeClockinSystem.ViewModels
{
    public class ScheduleViewModel
    {
        public int ScheduleId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsOff { get; set; } 
        public bool IsEndDateNextDay { get; set; }

    }
}