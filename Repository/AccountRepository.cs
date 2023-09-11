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
    public class AccountRepository : IAccountRepository
    {

        private readonly ApplicationDbContext _context;

        public AccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsers()
        {
            return await _context.Users.Include(e => e.Employee).ToListAsync();
        }

        public async Task<AppUser> GetUserById(string id)
        {
            return await _context.Users.Include(e => e.Employee).FirstOrDefaultAsync(u => u.Id == id);
        }

        public bool UpdateEmployee(Employee employee)
        {
            _context.Update(employee);
            return Save();
        }
        public bool AddEmployee(Employee employee)
        {
            _context.Add(employee);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

    }
}