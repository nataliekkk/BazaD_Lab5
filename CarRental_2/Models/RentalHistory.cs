using System;
using System.Collections.Generic;

namespace CarRental_2.Models;

public partial class RentalHistory
{
    public int RentalHistoryId { get; set; }

    public int ClientId { get; set; }

    public DateOnly StartDateTime { get; set; }

    public DateOnly ActualEndDateTime { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual Client Client { get; set; } = null!;
}
