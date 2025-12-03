namespace CarRental_2.ViewModels
{
    public class RentalHistoryItemViewModel
    {
        public int RentalHistoryId { get; set; }

        public string FullName { get; set; }

        public string LicenseNumber { get; set; }

        public string PhoneNumber { get; set; }

        public DateOnly StartDateTime { get; set; }

        public DateOnly ActualEndDateTime { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
