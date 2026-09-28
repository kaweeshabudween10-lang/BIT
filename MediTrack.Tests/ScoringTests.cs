using System;
using MediTrack.Core;
using MediTrack.Data;
using MediTrack.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediTrack.Tests
{
    public class ScoringTests
    {
        [Fact]
        public void TestRiskScoring_HighRiskPatient_CalculatesCorrectly()
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            int patientId;
            int doctorId;
            using (var db = new AppDbContext(options))
            {
                db.Database.EnsureCreated();

                var patient = new Patient
                {
                    FullName = "High Risk Patient",
                    ChronicConditionFlag = true
                };
                var doctor = new Doctor { Specialization = "General" };
                db.Patients.Add(patient);
                db.Doctors.Add(doctor);
                db.SaveChanges();

                patientId = patient.PatientId;
                doctorId = doctor.DoctorId;
                db.Appointments.AddRange(
                    new Appointment
                    {
                        PatientId = patientId,
                        DoctorId = doctorId,
                        DateTime = DateTime.Now.AddDays(-14),
                        Status = "No-show"
                    },
                    new Appointment
                    {
                        PatientId = patientId,
                        DoctorId = doctorId,
                        DateTime = DateTime.Now.AddDays(-7),
                        Status = "No-show"
                    });
                db.SaveChanges();
            }

            var service = new PatientRiskService(() => new AppDbContext(options));
            var riskItem = Assert.Single(service.GenerateFollowUpWatchlist());

            Assert.Equal(patientId, riskItem.PatientId);
            Assert.Equal(70, riskItem.CalculatedScore);
            Assert.Equal("High", riskItem.RiskLevel);
            Assert.Equal(2, riskItem.RiskReasons.Count);
        }
    }
}