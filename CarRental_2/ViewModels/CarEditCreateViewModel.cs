using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarRental_2.ViewModels
{
    public class CarEditCreateViewModel
    {
        public int CarId { get; set; }  // Hidden для Edit

       
        public int CarClassId { get; set; }

        
        public string Brand { get; set; } = string.Empty;

        
        public string Model { get; set; } = string.Empty;

        
        public string LicensePlate { get; set; } = string.Empty;

        
        public int Year { get; set; } = 1900;

        
        public decimal RentalCostPerDay { get; set; } = 0m;

        
        public string Status { get; set; } = "Доступен";
    }
}
