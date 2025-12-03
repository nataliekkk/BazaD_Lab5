using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CarRental_2.Models;
using CarRental_2.Data;
using Microsoft.EntityFrameworkCore;

namespace CarRental_2;

public partial class CarRental_2Context : IdentityDbContext<ApplicationUser>
{
    public CarRental_2Context()
    {
    }

    public CarRental_2Context(DbContextOptions<CarRental_2Context> options)
        : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }

    public  virtual DbSet<CarClass> CarClasses { get; set; }

    public  virtual DbSet<Client> Clients { get; set; }

    public  DbSet<Maintenance> Maintenances { get; set; }

    public  DbSet<RentalAgreement> RentalAgreements { get; set; }

    public DbSet<RentalHistory> RentalHistory { get; set; }
}
