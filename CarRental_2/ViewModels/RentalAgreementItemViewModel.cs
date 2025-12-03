using System.ComponentModel.DataAnnotations;

namespace CarRental_2.ViewModels
{
    public class RentalAgreementItemViewModel
    {
        public int RentalAgreementId { get; set; }

        public string FullName { get; set; }

        public string LicenseNumber { get; set; }

        public string PhoneNumber { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public DateOnly StartDateTime { get; set; }

        public DateOnly PlannedEndDateTime { get; set; }

        public DateOnly ActualEndDateTime { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
