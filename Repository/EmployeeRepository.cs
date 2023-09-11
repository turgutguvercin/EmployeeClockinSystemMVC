using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeClockinSystem.Data;
using EmployeeClockinSystem.Interfaces;
using EmployeeClockinSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeClockinSystem.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Employee> GetCurrentEmployee(string curUserId)
        {
            var employee = await _context.Employees
                                         .Include(e => e.Schedules)
                                         .Include(e => e.Records)
                                         .FirstOrDefaultAsync(e => e.AppUserId == curUserId);
            return employee;
        }


           public async Task<List<Employee>> GetAllEmployees()
        {
            return await _context.Employees.Include(e => e.Schedules)
                                           .Include(e => e.Records)
                                           .ToListAsync();
        }

        public async Task<ICollection<Schedule>> GetSchedulesByStartDate(DateTime startDate)
        {
            var endDate = startDate.AddDays(7); // Assuming a week's duration
            return await _context.Schedules.Include(e => e.Employee)
                .Where(s => s.StartDate >= startDate && s.EndDate <= endDate)
                .ToListAsync();
        }


        public Record GetLastClockInClockRecord(Employee employee)
        {
            return employee.Records
                              .OrderByDescending(record => record.ClockIn)
                              .FirstOrDefault();
        }

        public bool DeleteSchedulesRange(DateTime startDate)
        {
            DateTime endDate = startDate.AddDays(7);
            var schedulesToDelete = _context.Schedules
                .Where(s => s.StartDate >= startDate && s.StartDate < endDate);

            _context.Schedules.RemoveRange(schedulesToDelete);
            return Save();
        }



        public bool AddSchedule(Schedule schedule)
        {
            _context.Add(schedule);
            return Save();
        }

        public bool AddRecord(Record record)
        {

            _context.Records.Add(record);
            return Save();
        }

        public bool UpdateRecord(Record recordToUpdate)
        {

            _context.Update(recordToUpdate);
            return Save();
        }


        public bool Delete(Record record)
        {
            throw new NotImplementedException();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }



    }
}