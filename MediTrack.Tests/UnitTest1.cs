using MediTrack.Core;
using MediTrack.Data.Models;
using Xunit;

namespace MediTrack.Tests
{
    public class CoreServiceValidationTests
    {
        [Fact]
        public void BookAppointment_RejectsNullAppointment()
        {
            var result = new AppointmentService().BookAppointment(null!);

            Assert.False(result.Success);
        }

        [Fact]
        public void BookAppointment_RejectsPastAppointment()
        {
            var result = new AppointmentService().BookAppointment(new Appointment
            {
                PatientId = 1,
                DoctorId = 1,
                DateTime = DateTime.Now.AddMinutes(-1)
            });

            Assert.False(result.Success);
        }

        [Fact]
        public void HasSchedulingConflict_RejectsNonPositiveDuration()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new AppointmentService().HasSchedulingConflict(1, DateTime.Today, 0));
        }

        [Fact]
        public void DeductMedicineStock_RejectsNonPositiveQuantity()
        {
            var result = new BillingInventoryService().DeductMedicineStock(1, 0);

            Assert.False(result.Success);
        }

        [Fact]
        public void CreateInvoice_RejectsInvalidVisitId()
        {
            var saved = new BillingInventoryService().CreateInvoice(0, 100m, 25m, out var invoiceId);

            Assert.False(saved);
            Assert.Equal(0, invoiceId);
        }

        [Fact]
        public void RegisterUser_RejectsUnsupportedRole()
        {
            var result = new UserService().RegisterUser("test", "Test User", "password", "Visitor");

            Assert.False(result.Success);
        }
    }
}