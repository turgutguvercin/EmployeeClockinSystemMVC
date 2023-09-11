using EmployeeClockinSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmployeeClockinSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Record> Records { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Employee
            modelBuilder.Entity<Employee>()
                .Property(e => e.FullName)
                .IsRequired();

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.AppUser)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.AppUserId);

            modelBuilder.Entity<Employee>()
                .HasKey(e => e.EmployeeId);

            // Schedule
            modelBuilder.Entity<Schedule>()
                .HasKey(e => e.ScheduleId);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.Schedules)
                .HasForeignKey(s => s.EmployeeId);

            // Record
            modelBuilder.Entity<Record>()
                .HasKey(e => e.RecordId);

            modelBuilder.Entity<Record>()
                .HasOne(r => r.Employee)
                .WithMany(e => e.Records)
                .HasForeignKey(r => r.EmployeeId);

            base.OnModelCreating(modelBuilder);
        }


    }


}