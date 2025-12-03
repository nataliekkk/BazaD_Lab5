using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarRental_2.Areas.Identity.Pages.Account
{
    public class LogoutFailedModel : PageModel
    {
        public string Message { get; private set; }

        public void OnGet(string message)
        {
            Message = message ?? "Неизвестная ошибка."; // Устанавливаем сообщение, если не передано
        }
    }
}
