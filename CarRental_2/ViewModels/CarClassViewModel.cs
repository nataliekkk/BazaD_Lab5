using CarRental_2.Models;

namespace CarRental_2.ViewModels
{
    public class CarClassViewModel
    {
        public IEnumerable<CarClass> CarClasses { get; set; }

        //Свойство для поиска
        public string SearchName { get; set; }

        //Свойство для навигации по страницам
        public PageViewModel PageViewModel { get; set; }
    }
}
