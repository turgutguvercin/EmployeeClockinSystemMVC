using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeClockinSystem.Interfaces;
using EmployeeClockinSystem.Models;
using EmployeeClockinSystem.Repository;
using Microsoft.AspNetCore.Identity;

namespace EmployeeClockinSystem.Middleware
{

    public class EmployeeCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public EmployeeCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // if a user is not defined as an employee
        public async Task InvokeAsync(HttpContext context, IEmployeeRepository employeeRepository, UserManager<AppUser> userManager)
        {
            // Check the path to ensure the middleware doesn't run for the NotAnEmployee route
            if (context.Request.Path.Value.Equals("/Home/NotAnEmployee", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
              if (context.Request.Path.Value.Equals("/Account/Logout", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var user = await userManager.GetUserAsync(context.User);
            if (user != null)
            {
                var employee = await employeeRepository.GetCurrentEmployee(user.Id);
                if (employee == null)
                {
                    context.Response.Redirect("/Home/NotAnEmployee");
                    return;
                }
            }
            await _next(context);
        }



    }
    public static class EmployeeCheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseEmployeeCheck(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EmployeeCheckMiddleware>();
        }
    }


}