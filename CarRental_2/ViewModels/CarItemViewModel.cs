namespace CarRental_2.ViewModels
{
    public class CarItemViewModel
    {
        public int CarId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal RentalCostPerDay { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
