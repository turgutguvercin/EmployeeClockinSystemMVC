using EmployeeClockinSystem.Interfaces;
using EmployeeClockinSystem.Models;
using EmployeeClockinSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeClockinSystem.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocationService _locationService;

        public EmployeeController(IEmployeeRepository employeeRepository, IHttpContextAccessor httpContextAccessor, ILocationService locationService)
        {
            _employeeRepository = employeeRepository;
            _httpContextAccessor = httpContextAccessor;
            _locationService = locationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ClockIn([FromBody] GeolocationDto geo)
        {
            // Retrieve the current user's ID from the HTTP context.
            var currentUserId = _httpContextAccessor.HttpContext?.User.GetUserId();

            // Check if the user is recognized. Return an error if not.
            if (currentUserId == null)
            {
                return BadRequest(new { Message = "User not recognized." });
            }

            // Fetch the employee details for the current user.
            var employee = await _employeeRepository.GetCurrentEmployee(currentUserId);

            // Check if the employee is present in the database. Return an error if not.
            if (employee == null)
            {
                return NotFound(new { Message = "Employee not found." });
            }

            // Initialize the lastPunch variable. This will store the latest record of when the employee clocked in.
            Record lastPunch = null;

            // Try to fetch the latest clock-in record for the employee. 
            // It's wrapped in a try-catch to handle possible exceptions like database connectivity issues.
            try
            {
                lastPunch = _employeeRepository.GetLastClockInClockRecord(employee);
            }
            catch (Exception ex)
            {
                // Log any exception encountered. Depending on your application's logging strategy, this could be
                Console.WriteLine($"Error while fetching last clock record: {ex.Message}");
            }

            // Check if the employee is within an allowed location to clock in.
            bool isWithinAllowedLocation = _locationService.IsWithinAllowedLocation(geo.Latitude, geo.Longitude);
            if (!isWithinAllowedLocation)
            {
                return Ok(new { Message = "You are not in the allowed location!" });
            }

            // Check if the employee can clock in. They can only clock in if:
            // 1. They haven't clocked in before (lastPunch is null) or
            // 2. Their last clock-in has a corresponding clock-out (they've ended their previous shift).
            bool canClockIn = lastPunch == null || lastPunch.ClockOut.HasValue;

            if (canClockIn)
            {
                // Create a new clock-in record and add it to the database.
                var record = new Record
                {
                    EmployeeId = employee.EmployeeId,
                    ClockIn = DateTime.Now
                };
                _employeeRepository.AddRecord(record);
            }

            // Return a response indicating if the employee was able to clock in or if they were already clocked in.
            return Ok(new
            {
                Message = canClockIn ? "Clocked In!" : "Already Clocked In, but haven't Clocked Out yet!",
                LastPunchType = "ClockIn",
                LastPunchTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ClockOut([FromBody] GeolocationDto geo)
        {
            // Retrieve the current user's ID from the HTTP context.
            var currentUserId = _httpContextAccessor.HttpContext?.User.GetUserId();

            // Ensure user ID exists.
            if (currentUserId == null)
            {
                return BadRequest(new { Message = "User not recognized." });
            }

            // Fetch the employee details for the current user.
            var employee = await _employeeRepository.GetCurrentEmployee(currentUserId);

            // Ensure employee exists.
            if (employee == null)
            {
                return NotFound(new { Message = "Employee not found." });
            }

            // Fetch the latest clock-in record for the employee.
            Record lastPunch = _employeeRepository.GetLastClockInClockRecord(employee);


            //Console.WriteLine($"Geo: {geo.Latitude}, {geo.Longitude}. Last Punch: {lastPunch}");

            // Check if the employee is within an allowed location to clock out.
            if (_locationService.IsWithinAllowedLocation(geo.Latitude, geo.Longitude))
            {
                // Verify that the employee has clocked in and not yet clocked out.
                if (lastPunch != null && !lastPunch.ClockOut.HasValue)
                {
                    // Update the last clock-in record's clock-out time.
                    lastPunch.ClockOut = DateTime.Now;
                    _employeeRepository.UpdateRecord(lastPunch);

                    return Ok(new
                    {
                        Message = "Clocked Out!",
                        LastPunchType = "ClockOut",
                        LastPunchTimestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                    });
                }
                else
                {
                    // This block handles two scenarios:
                    // 1. The employee hasn't clocked in yet for their current shift.
                    // 2. The employee already clocked out after their last clock-in.
                    return Ok(new
                    {
                        Message = "Error! Either you haven't clocked in for your current shift or you already clocked out.",
                        LastPunchType = lastPunch != null ? "ClockOut" : "None",
                        LastPunchTimestamp = lastPunch?.ClockOut?.ToString("dd/MM/yyyy HH:mm:ss") ?? "N/A"
                    });
                }
            }
            else
            {
                // The employee is not within the allowed location to clock out.
                return Ok(new { Message = "You are not in the allowed location!" });
            }
        }


        [Authorize]
        public async Task<IActionResult> GetSchedule(DateTime? startDate, DateTime? endDate)
        {
            // Get the current user ID. If it's null, it makes no sense to proceed.
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();
            if (string.IsNullOrEmpty(curUserId))
            {
                // Handle this case appropriately; for instance, redirecting or returning an error.
                return RedirectToAction("Index", "Home"); // This is just an example, adjust as necessary.
            }

            // If the employee doesn't exist or the data is null, we need to handle that as well.
            var employee = await _employeeRepository.GetCurrentEmployee(curUserId);
            if (employee == null)
            {
                // Handle this case as well.
                return NotFound("Employee not found");
            }

            // Determine the start and end dates.
            DateTime start = startDate ?? DateTime.Now.AddDays(1 - (int)DateTime.Now.DayOfWeek);
            DateTime end = endDate ?? start.AddDays(7);

            // Filter the schedules for the given date range. 
            // This avoids sending unnecessary data to the frontend.
            var filteredSchedules = employee.Schedules
                                          .Where(s => s.StartDate >= start && s.EndDate <= end)
                                          .ToList();

            // Populate the ViewModel.
            var employeeWeeklyVM = new EmployeeWeeklyViewModel()
            {
                EmployeeName = employee.FullName,
                StartDate = start,
                EndDate = end,
                Schedules = filteredSchedules
            };

            return View(employeeWeeklyVM);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ShowWeeklyScheduleForAllEmployees(DateTime? startDate, DateTime? endDate)
        {
            // Fetch all the employees. 
            var employees = await _employeeRepository.GetAllEmployees();

            if (employees == null || !employees.Any())
            {

                return NotFound("No employees found");
            }

            // If startDate isn't provided, set it to the beginning of the current week.
            DateTime start = startDate ?? DateTime.Now.AddDays(1 - (int)DateTime.Now.DayOfWeek);

            // If endDate isn't provided, set it to one week from the start date.
            DateTime end = endDate ?? start.AddDays(7);

            // Populate the ViewModel.
            var weeklyscheduleVM = new WeeklyScheduleViewModel()
            {
                StartDate = start,
                EndDate = end,
                Employees = employees
            };

            return View(weeklyscheduleVM);
        }


        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateSchedule(DateTime? userDefinedStartDate)
        {
            // Set as default to the beginning of the next week.
            DateTime startDate = userDefinedStartDate ?? DateTime.Now.AddDays(1 - (int)DateTime.Now.DayOfWeek).AddDays(7);

            // Pass the startDate to the view.
            ViewBag.StartDate = startDate;

            // Fetch schedules that match the start date.
            var schedules = await _employeeRepository.GetSchedulesByStartDate(startDate);

            // Create a dictionary to help in looking up schedule data in O(1) time.
            var scheduleLookup = new Dictionary<string, ScheduleViewModel>();

            // Create a list to hold all the schedule.
            ICollection<ScheduleViewModel> postedSchedules = new List<ScheduleViewModel>();

            // Loop through the schedules and create view models for each.
            foreach (var schedule in schedules)
            {
                var viewModel = new ScheduleViewModel
                {
                    ScheduleId = schedule.ScheduleId,
                    EmployeeId = schedule.EmployeeId,
                    StartDate = schedule.StartDate,
                    EndDate = schedule.EndDate
                };

                // Use a combination of EmployeeId and StartDate as a unique key for each schedule in lookup.
                string key = $"{schedule.EmployeeId}-{schedule.StartDate:yyyy-MM-dd}";
                scheduleLookup[key] = viewModel;

                // Add the view model to our list.
                postedSchedules.Add(viewModel);
            }

            // Fetch all employees.
            var employees = await _employeeRepository.GetAllEmployees();

            // Pass the employees to the view.
            ViewData["employees"] = employees;

            // Populate the main view model.
            var createScheduleVM = new CreateScheduleViewModel
            {
                ScheduleLookup = scheduleLookup,
                PostedSchedules = postedSchedules,
                StartDateOfWeek = startDate
            };

            // Return the populated view model to the view.
            return View(createScheduleVM);
        }


        [HttpPost]
        [Authorize]
        public IActionResult SaveSchedule(CreateScheduleViewModel createScheduleVM)
        {
            // Ensure the ViewModel is not null.
            if (createScheduleVM == null)
            {
                return BadRequest("Invalid data provided.");
            }

            // Ensure the start date from ViewModel is in the desired format.
            DateTime startDate = createScheduleVM.StartDateOfWeek.Date; // Extracts the date part only.

            // Delete existing schedules for the specified start date.
            // It's important to be aware of the implications of deleting records. 
            // Rollback needs to be added here.
            _employeeRepository.DeleteSchedulesRange(startDate);

            // Loop through the schedules provided in the ViewModel.
            foreach (var scheduleVM in createScheduleVM.PostedSchedules)
            {
                // If the employee is not off that day, create a new schedule entry.
                if (!scheduleVM.IsOff)
                {
                    var schedule = new Schedule()
                    {
                        EmployeeId = scheduleVM.EmployeeId,
                        StartDate = scheduleVM.StartDate,
                        EndDate = scheduleVM.EndDate,
                        Shift = "Regular"  // This can be further customised if you have other shift types.
                    };

                    // Add the newly created schedule to the repository.
                    _employeeRepository.AddSchedule(schedule);
                }
            }

            // Send a temporary success message.
            TempData["SuccessMessage"] = "Create/Update successful!";

            return RedirectToAction("CreateSchedule", "Employee");
        }


        [Authorize]
        public async Task<IActionResult> WeeklyRecords(DateTime? startDate, DateTime? endDate)
        {
            // Retrieve the current user ID
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();

            // Fetch the employee record based on the user ID
            var employee = await _employeeRepository.GetCurrentEmployee(curUserId);

            // If the employee is not found, return a NotFound result
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            // If no start date is provided, default to the start of the current week
            DateTime startOfWeek = DateTime.Now.AddDays(1 - (int)DateTime.Now.DayOfWeek);
            DateTime start = startDate ?? startOfWeek;

            // If no end date is provided, default to one week after the start date
            DateTime end = endDate ?? start.AddDays(7);

            // Filter the records and schedules based on the date range
            var filteredRecords = employee.Records
                                          .Where(r => r.ClockIn.Date >= start.Date && r.ClockIn.Date < end.Date)
                                          .ToList();

            var filteredSchedules = employee.Schedules
                                           .Where(s => s.StartDate.Date >= start.Date && s.EndDate.Date < end.Date)
                                           .ToList();

            // Populate the ViewModel with the necessary details
            var employeeWeeklyVM = new EmployeeWeeklyViewModel()
            {
                EmployeeName = employee.FullName,
                StartDate = start,
                EndDate = end,
                Records = filteredRecords,
                Schedules = filteredSchedules
            };

            // Return the View with the populated ViewModel
            return View(employeeWeeklyVM);
        }


        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ShowAllWeeklyRecords(DateTime? startDate, DateTime? endDate)
        {
            // Retrieve all employees
            var employees = await _employeeRepository.GetAllEmployees();

            // If no start date is provided, default to the start of the current week
            DateTime startOfWeek = DateTime.Now.AddDays(1 - (int)DateTime.Now.DayOfWeek);
            startDate ??= startOfWeek;  // Using the '??=' operator to assign the value if it's null

            // If no end date is provided, default to one week after the start date
            endDate ??= startDate.Value.AddDays(7);

            // Populate the ViewBag for use in the view
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // Create a list of ViewModels for each employee, populated with their weekly data
            var employeesWeeklyVM = employees.Select(employee => new EmployeeWeeklyViewModel()
            {
                EmployeeName = employee.FullName,
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                Records = employee.Records.Where(r => r.ClockIn.Date >= startDate && r.ClockIn.Date <= endDate).ToList(),
                Schedules = employee.Schedules.Where(s => s.StartDate.Date >= startDate && s.StartDate.Date <= endDate).ToList()
            }).ToList();

            // Return the populated list of models to the view
            return View(employeesWeeklyVM);
        }


    }


    public class GeolocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
