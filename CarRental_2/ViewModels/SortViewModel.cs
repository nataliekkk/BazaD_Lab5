namespace CarRental_2.ViewModels
{
    public enum SortState
    {
        No, // не сортировать
        RentalCostPerDayAsc,    // по возрастанию
        RentalCostPerDayDesc,   // по убыванию
        TotalAmountAsc,         // по возрастанию
        TotalAmountDesc,        // по убыванию
        CostAsc,                // по возрастанию
        CostDesc,                // по по убыванию
        PhoneNumberAsc,
        PhoneNumberDesc
    }
    public class SortViewModel
    {
        public SortState RentalCostPerDaySort { get; set; }
        public SortState TotalAmountSort { get; set; }
        public SortState CostSort { get; set; }
        public SortState PhoneNumberSort { get; set; }

        public SortState CurrentState { get; set; }
        public SortState SortOrder => CurrentState;

        public SortViewModel(SortState sortOrder)
        {
            RentalCostPerDaySort = sortOrder == SortState.RentalCostPerDayAsc ? SortState.RentalCostPerDayDesc : SortState.RentalCostPerDayAsc;
            TotalAmountSort = sortOrder == SortState.TotalAmountAsc ? SortState.TotalAmountDesc : SortState.TotalAmountAsc;
            CostSort = sortOrder == SortState.CostAsc ? SortState.CostDesc : SortState.CostAsc;
            PhoneNumberSort = sortOrder == SortState.PhoneNumberAsc ? SortState.PhoneNumberDesc : SortState.PhoneNumberAsc;

            CurrentState = sortOrder;
        }
    }
}
