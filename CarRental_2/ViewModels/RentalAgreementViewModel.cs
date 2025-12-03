using CarRental_2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarRental_2.ViewModels
{
    public class RentalAgreementViewModel
    {
        public IEnumerable<RentalAgreementItemViewModel> RentalAgreements { get; set; } = Enumerable.Empty<RentalAgreementItemViewModel>();

        //Свойство для навигации по страницам
        public PageViewModel PageViewModel { get; set; }

        // Порядок сортировки
        public SortViewModel SortViewModel { get; set; }

        //Свойство для фильтрации
        public decimal TotalAmount { get; set; }
    }
}
