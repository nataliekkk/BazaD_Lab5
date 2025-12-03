using System;
using System.Collections.Generic;

namespace CarRental_2.Models;

public partial class Car
{
    public int CarId { get; set; }

    public int CarClassId { get; set; }

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string LicensePlate { get; set; } = null!;

    public int Year { get; set; }

    public decimal RentalCostPerDay { get; set; }

    public string Status { get; set; } = null!;

    public virtual CarClass Class { get; set; } = null!;

    public virtual ICollection<Maintenance> Maintenances { get; set; } = new List<Maintenance>();

    public virtual ICollection<RentalAgreement> RentalAgreements { get; set; } = new List<RentalAgreement>();
}
