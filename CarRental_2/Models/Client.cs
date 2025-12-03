using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarRental_2.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string FullName { get; set; }

    public string LicenseNumber { get; set; }

    public string PhoneNumber { get; set; }

    public virtual ICollection<RentalAgreement> RentalAgreements { get; set; } = new List<RentalAgreement>();

    public virtual ICollection<RentalHistory> RentalHistories { get; set; } = new List<RentalHistory>();
}
