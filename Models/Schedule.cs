using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeClockinSystem.Models
{
    public class Schedule
    {
        
        public int ScheduleId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Shift { get; set; }
        public virtual Employee Employee { get; set; }
    }
}