using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using CarRental_2;

namespace CarRental_2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Дата регистрации")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; }


    }
}
