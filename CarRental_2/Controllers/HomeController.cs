using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
namespace CarRental_2.Controllers
{
        public class HomeController : Controller
        {
        public IActionResult Index()
        {
            return View();
        }
    }
}
