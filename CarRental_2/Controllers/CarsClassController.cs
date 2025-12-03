using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CarRental_2.Controllers
{
    [Authorize]
    public class CarsClassController : Controller
    {
        private readonly int _pageSize;
        private readonly CarRental_2Context _context;
        private readonly ILogger<CarsClassController> _logger;
        private const string CAR_CLASS_SEARCH_COOKIE_NAME = "CarClassSearchName";

        public CarsClassController(CarRental_2Context context, ILogger<CarsClassController> logger, IConfiguration appConfig = null)
        {
            _context = context;
            _logger = logger;
            _pageSize = appConfig != null && int.TryParse(appConfig["Parameters:PageSize"], out int pageSizeConfig)
                ? pageSizeConfig : 20;
        }

        public async Task<IActionResult> Index(string SearchName = "", int page = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Приоритет query → куки → ""
            string filterSearchName = Request.Query.ContainsKey("SearchName") ? (SearchName ?? "") : GetSearchNameFromCookie();

            // Запрос с фильтром
            var carClassesQuery = _context.CarClasses.AsQueryable();
            if (!string.IsNullOrEmpty(filterSearchName))
            {
                carClassesQuery = carClassesQuery.Where(c => c.Name.Contains(filterSearchName));
            }

            // Статистика
            var totalAll = await _context.CarClasses.CountAsync();
            var count = await carClassesQuery.CountAsync();

            // Пагинация
            var pagedCarClasses = await carClassesQuery
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            var ccViewModel = new CarClassViewModel
            {
                CarClasses = pagedCarClasses,
                SearchName = filterSearchName,
                PageViewModel = new PageViewModel(count, page, _pageSize)
            };

            // ViewBag (всегда!)
            ViewBag.TotalAllRecords = totalAll;
            ViewBag.FilterSearchName = string.IsNullOrEmpty(filterSearchName) ? "Все классы" : $"\"{filterSearchName}\"";
            ViewBag.HasFilters = !string.IsNullOrEmpty(filterSearchName);
            ViewBag.ResetUrl = Url.Action("Index");

            SetSearchNameCookie(filterSearchName);

            return View(ccViewModel);
        }

        private string GetSearchNameFromCookie()
        {
            Request.Cookies.TryGetValue(CAR_CLASS_SEARCH_COOKIE_NAME, out string value);
            return value ?? "";
        }

        private void SetSearchNameCookie(string searchName)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = Request.Scheme == "https"
            };
            Response.Cookies.Append(CAR_CLASS_SEARCH_COOKIE_NAME, searchName, cookieOptions);
        }


        // GET: CarClass/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: CarClass/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, Description")] CarClass carClass)
        {
            if (ModelState.IsValid)
            {
                _context.Add(carClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Перенаправление на метод Index после успешного добавления
            }

            // Если модель не валидна, возвращаем форму с сообщениями об ошибках
            return View(carClass);
        }

        // GET: CarClass/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carClass = await _context.CarClasses.SingleOrDefaultAsync(m => m.CarClassId == id);
            if (carClass == null)
            {
                return NotFound();
            }

            return View(carClass);
        }

        // POST: СarClass/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CarClassId, Name, Description")] CarClass carClass)
        {
            if (id != carClass.CarClassId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(carClass);
            }
            else
            {
                try
                {
                    _context.Update(carClass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarClassExists(carClass.CarClassId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

        }

        // GET: CarClass/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var carClass = await _context.CarClasses
                .SingleOrDefaultAsync(m => m.CarClassId == id);
            if (carClass == null)
            {
                return NotFound();
            }

            return View(carClass);
        }

        // POST: CarClass/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var carClass = await _context.CarClasses.SingleOrDefaultAsync(m => m.CarClassId == id);
            _context.CarClasses.Remove(carClass);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarClassExists(int id)
        {
            return _context.CarClasses.Any(e => e.CarClassId == id);
        }

        private static IQueryable<CarClass> SearchByClassName(IQueryable<CarClass> carClasses, string SearchName)
        {
            // Фильтрация по номеру телефона
            if (!string.IsNullOrEmpty(SearchName)) // Проверяем, что phoneNumber не пустой
            {
                carClasses = carClasses.Where(o => o.Name != null && o.Name.Contains(SearchName));
            }

            return carClasses;
        }
    }
}