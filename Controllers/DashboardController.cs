using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeClockinSystem.Interfaces;
using EmployeeClockinSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EmployeeClockinSystem.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IDashboardRepository _dashboardRepository;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(IDashboardRepository dashboardRepository, UserManager<AppUser> userManager)
        {
            _dashboardRepository = dashboardRepository;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            

            var employee = await _dashboardRepository.GetByUserId(user.Id);
            if (employee == null)
            {
                return RedirectToAction("NotAnEmployee", "Error");
            }
            
         

            // Fetch the last punch.
            Record? lastPunch = null;
            try
            {
                lastPunch = _dashboardRepository.GetLastClockInClockRecord(employee);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (lastPunch != null)
            {
                if (!lastPunch.ClockOut.HasValue)
                {
                    ViewBag.LastPunch = "ClockIn: " + lastPunch.ClockIn.ToString("dd/MM/yyyy HH:mm:ss");
                    ViewBag.HasClockedIn = true;
                }
                else
                {
                    ViewBag.LastPunch = "ClockOut: " + lastPunch.ClockOut.Value.ToString("dd/MM/yyyy HH:mm:ss");
                    ViewBag.HasClockedIn = false;
                }
            }
            else
            {
                ViewBag.LastPunch = "No previous record found. If you think there is a problem contact your manager.";
                ViewBag.hasClockedIn = false;
            }

            return View(employee);
        }

      
    }
}