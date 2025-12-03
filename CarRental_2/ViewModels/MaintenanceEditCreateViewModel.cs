using CarRental_2.Models;

namespace CarRental_2.ViewModels
{
    public class MaintenanceEditCreateViewModel
    {
        public int Id { get; set; }

        public int CarId { get; set; }

        public DateOnly MaintenanceDate { get; set; }

        public string Description { get; set; } = string.Empty!;

        public decimal Cost { get; set; }
    }
}
