using EmployeeClockinSystem.Data;
using EmployeeClockinSystem.Interfaces;
using EmployeeClockinSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeClockinSystem.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> GetByUserId(string userId)
        {
            return await _context.Employees.Include(e => e.Records).FirstOrDefaultAsync(e => e.AppUserId == userId);
        }


        public Record GetLastClockInClockRecord(Employee employee)
        {
            return employee.Records
                              .OrderByDescending(record => record.ClockIn)
                              .First();
        }

    }
}