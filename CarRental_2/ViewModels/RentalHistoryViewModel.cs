using System;
using System.Collections.Generic;

namespace CarRental_2.ViewModels;

public partial class RentalHistoryViewModel
{
    public IEnumerable<RentalHistoryItemViewModel> RentalHistories { get; set; } = Enumerable.Empty<RentalHistoryItemViewModel>();

    //Свойство для навигации по страницам
    public PageViewModel PageViewModel { get; set; }

    //Свойство для фильтрации
    public decimal TotalAmount { get; set; }
}
