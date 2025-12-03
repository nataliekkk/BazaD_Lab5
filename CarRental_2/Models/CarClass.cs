using System;
using System.Collections.Generic;

namespace CarRental_2.Models;

public partial class CarClass
{
    public int CarClassId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
