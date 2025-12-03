using System;
using System.Collections.Generic;

namespace CarRental_2.Models;

public partial class Maintenance
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public DateOnly MaintenanceDate { get; set; }

    public string Description { get; set; } = null!;

    public decimal Cost { get; set; }

    public virtual Car Car { get; set; } = null!;
}
