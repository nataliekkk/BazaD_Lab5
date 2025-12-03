using CarRental_2.Models;

namespace CarRental_2.ViewModels
{
    public class MaintenanceItemViewModel
    {
        public int Id { get; set; }

        public string Brand { get; set; } = string.Empty;

        public string Model { get; set; } = string.Empty;

        public DateOnly MaintenanceDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Cost { get; set; }
    }
}
