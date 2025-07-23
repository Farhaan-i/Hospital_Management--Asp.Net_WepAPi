using HMS.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HMS
{
    public class AppDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    // Replace connection string with your actual database details
        //    optionsBuilder.UseSqlServer("Data Source=LTIN593801;Initial Catalog=FinalHospApp;Integrated Security=True;TrustServerCertificate=true");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict); ;
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<MedicalHistory>()
                .HasOne(m => m.Patient)
                .WithMany(p => p.MedicalHistories)
                .HasForeignKey(m => m.PatientId);

            modelBuilder.Entity<Slot>()
           .HasOne(s => s.Doctor) // Each slot has one doctor
           .WithMany(d => d.Slots) // Each doctor has many slots
           .HasForeignKey(s => s.DoctorId) // Foreign key in Slot
           .OnDelete(DeleteBehavior.Cascade); // Cascade delete when Doctor is deleted

            // Patient and Slot (Optional One-to-Many relationship)
            modelBuilder.Entity<Slot>()
                .HasOne(s => s.Patient) // Each slot can have one patient (if booked)
                .WithMany(p => p.BookedSlots) // Each patient can book multiple slots
                .HasForeignKey(s => s.PatientId) // Foreign key in Slot
                .OnDelete(DeleteBehavior.Cascade);
            // Slot and Appointment(One-to - One Relationship)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Slot)
                .WithMany(s => s.Appointments) // Correct: Appointments should be a list
                .HasForeignKey(a => a.SlotId)
                .OnDelete(DeleteBehavior.Cascade);

           




        }
    }
}
