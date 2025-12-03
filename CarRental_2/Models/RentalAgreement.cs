using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CarRental_2.Models
{

    public partial class RentalAgreement
    {
        public int RentalAgreementId { get; set; }

        public int ClientId { get; set; }

        public int CarId { get; set; }

        public DateOnly StartDateTime { get; set; }

        public DateOnly PlannedEndDateTime { get; set; }

        public DateOnly ActualEndDateTime { get; set; }

        public decimal TotalAmount { get; set; }

        public virtual Car Car { get; set; } = null!;

        public virtual Client Client { get; set; } = null!;
    }
}
