namespace CarRental_2.ViewModels
{
    //Класс для хранения информации о страницах разбиения
    public class PageViewModel
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public PageViewModel(int totalItems, int page = 1, int pageSize = 20)
        {
            TotalItems = totalItems;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            Page = page;
        }

        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }
}
