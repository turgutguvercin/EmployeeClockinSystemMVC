using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeClockinSystem.Models;

namespace EmployeeClockinSystem.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<AppUser>> GetAllUsers();
        Task<AppUser> GetUserById(string id);
        bool AddEmployee(Employee employee);
        bool UpdateEmployee(Employee employee);
        bool Save();
    }
}