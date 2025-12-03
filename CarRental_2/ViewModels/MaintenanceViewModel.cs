namespace CarRental_2.ViewModels
{
    public class MaintenanceViewModel
    {
        public IEnumerable<MaintenanceItemViewModel> Maintenances { get; set; } = Enumerable.Empty<MaintenanceItemViewModel>();

        //Свойство для фильтрации
        public decimal Cost { get; set; }

        //Свойство для навигации по страницам
        public PageViewModel PageViewModel { get; set; }

        // Порядок сортировки
        public SortViewModel SortViewModel { get; set; }
    }
}
