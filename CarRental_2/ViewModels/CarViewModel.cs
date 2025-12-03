namespace CarRental_2.ViewModels
{
    public class CarViewModel
    {
        public IEnumerable<CarItemViewModel> Cars { get; set; } = Enumerable.Empty<CarItemViewModel>();

        //Свойство для навигации по страницам
        public PageViewModel PageViewModel { get; set; }

        // Порядок сортировки
        public SortViewModel SortViewModel { get; set; }

        public decimal RentalCostPerDay { get; set; }
    }
}
