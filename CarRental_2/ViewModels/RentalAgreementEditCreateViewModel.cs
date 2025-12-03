
namespace CarRental_2.ViewModels
{
    public class RentalAgreementEditCreateViewModel
    {
        public int RentalAgreementId { get; set; }

        public int ClientId { get; set; }

        public int CarId { get; set; }

        public DateOnly StartDateTime { get; set; }

        public DateOnly PlannedEndDateTime { get; set; }

        public DateOnly ActualEndDateTime { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
