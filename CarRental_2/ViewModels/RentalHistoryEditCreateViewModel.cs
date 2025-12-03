namespace CarRental_2.ViewModels
{
    public class RentalHistoryEditCreateViewModel
    {
        public int RentalHistoryId { get; set; }

        public int ClientId { get; set; }

        public DateOnly StartDateTime { get; set; }

        public DateOnly ActualEndDateTime { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
