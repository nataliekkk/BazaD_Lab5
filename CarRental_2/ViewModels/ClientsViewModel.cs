using System.ComponentModel.DataAnnotations;
using CarRental_2.Models;

namespace CarRental_2.ViewModels
{
    public class ClientsViewModel
    {
        public IEnumerable<Client> Clients { get; set; }
        
        //Свойство для фильтрации
        public string PhoneNumber { get; set; }
        
        //Свойство для навигации по страницам
        public PageViewModel PageViewModel { get; set; }

        // Порядок сортировки
        public SortViewModel SortViewModel { get; set; }
    }
}
