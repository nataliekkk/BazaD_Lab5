using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CarRental_2.Controllers
{
    [Authorize]
    public class RentalHistoriesController : Controller
    {
        private readonly int _pageSize;
        private readonly CarRental_2Context _context;
        private readonly ILogger<RentalHistoriesController> _logger;
        private const string RENTAL_HISTORY_TOTAL_AMOUNT_COOKIE_NAME = "RentalHistoryTotalAmount";

        public RentalHistoriesController(CarRental_2Context context, ILogger<RentalHistoriesController> logger, IConfiguration appConfig = null)
        {
            _context = context;
            _logger = logger;

            // Загрузка pageSize из конфигурации
            _pageSize = appConfig != null && int.TryParse(appConfig["Parameters:PageSize"], out int pageSizeConfig)
                ? pageSizeConfig
                : 20;
        }

        // GET: RentalHistories
        public async Task<IActionResult> Index(decimal totalAmount, int page = 1)
        {
            // Проверка, аутентифицирован ли пользователь
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Приоритет query → куки → 0
            decimal filterTotalAmount = Request.Query.ContainsKey("totalAmount") ? totalAmount : GetTotalAmountFromCookie();

            // Полный запрос БЕЗ фильтра для totalAll
            var fullQuery = from rental in _context.RentalHistory
                            join client in _context.Clients on rental.ClientId equals client.ClientId
                            select new RentalHistoryItemViewModel
                            {
                                RentalHistoryId = rental.RentalHistoryId,
                                FullName = client.FullName,
                                LicenseNumber = client.LicenseNumber,
                                PhoneNumber = client.PhoneNumber,
                                StartDateTime = rental.StartDateTime,
                                ActualEndDateTime = rental.ActualEndDateTime,
                                TotalAmount = rental.TotalAmount
                            };

            var totalAll = await fullQuery.CountAsync(); // Всего без фильтров

            // Фильтрованный запрос
            var rentalHistoriesQuery = fullQuery;
            rentalHistoriesQuery = Sort_Search(rentalHistoriesQuery, filterTotalAmount);

            // Разбиение на страницы
            var count = await rentalHistoriesQuery.CountAsync();
            var pagedRentalHistories = await rentalHistoriesQuery.Skip((page - 1) * _pageSize).Take(_pageSize).ToListAsync();

            // Формирование модели для передачи представлению
            var rhViewModel = new RentalHistoryViewModel
            {
                RentalHistories = pagedRentalHistories,
                PageViewModel = new PageViewModel(count, page, _pageSize),
                TotalAmount = filterTotalAmount
            };

            // ViewBag для инфо-блока
            ViewBag.TotalAllRecords = totalAll;
            ViewBag.FilterTotalAmount = filterTotalAmount > 0 ? $"<= {filterTotalAmount}" : "Все суммы";
            ViewBag.HasFilters = filterTotalAmount > 0;
            ViewBag.ResetUrl = Url.Action("Index");

            // Сохранение в куки
            SetTotalAmountCookie(filterTotalAmount);

            return View(rhViewModel);
        }


        // GET: RentalHistories/Create

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RentalHistories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalHistoryEditCreateViewModel rh)
        {
            if (ModelState.IsValid)
            {
                var rentalHistory = new RentalHistory
                {
                    ClientId = rh.ClientId,
                    StartDateTime = rh.StartDateTime,
                    ActualEndDateTime = rh.ActualEndDateTime,
                    TotalAmount = rh.TotalAmount
                };

                _context.Add(rentalHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Перенаправление на метод Index после успешного добавления
            }

            // Если модель не валидна, возвращаем форму с сообщениями об ошибках
            return View(rh);
        }

        // GET: RentalHistories/Edit/5

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var rentalHistory = await _context.RentalHistory.FindAsync(id);
            if (rentalHistory == null)
            {
                return NotFound();
            }

            var rh = new RentalHistoryEditCreateViewModel
            {
                RentalHistoryId = rentalHistory.RentalHistoryId,
                ClientId = rentalHistory.ClientId,
                StartDateTime = rentalHistory.StartDateTime,
                ActualEndDateTime = rentalHistory.ActualEndDateTime,
                TotalAmount = rentalHistory.TotalAmount
            };

            return View(rh);
        }

        // POST: RentalHistories/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RentalHistoryEditCreateViewModel rh)
        {
            if (id != rh.RentalHistoryId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(rh);
            }
            else
            {
                try
                {
                    var rentalHistory = await _context.RentalHistory.FindAsync(id);
                    if (rentalHistory == null) return NotFound();

                    rentalHistory.ClientId = rh.ClientId;
                    rentalHistory.StartDateTime = rh.StartDateTime;
                    rentalHistory.ActualEndDateTime = rh.ActualEndDateTime;
                    rentalHistory.TotalAmount = rh.TotalAmount;

                    _context.Update(rentalHistory);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalHistoryExists(rh.RentalHistoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        // GET: RentalHistories/Delete/5

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalHistory = await _context.RentalHistory
                .SingleOrDefaultAsync(m => m.RentalHistoryId == id);
            if (rentalHistory == null)
            {
                return NotFound();
            }

            return View(rentalHistory);
        }

        // POST: RentalHistories/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rentalHistory = await _context.RentalHistory.SingleOrDefaultAsync(m => m.RentalHistoryId == id);
            _context.RentalHistory.Remove(rentalHistory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private decimal GetTotalAmountFromCookie()
        {
            if (Request.Cookies.TryGetValue(RENTAL_HISTORY_TOTAL_AMOUNT_COOKIE_NAME, out string value) && decimal.TryParse(value, out decimal amount))
            {
                return amount;
            }
            return 0m;
        }

        private void SetTotalAmountCookie(decimal totalAmount)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = Request.Scheme == "https"
            };

            Response.Cookies.Append(RENTAL_HISTORY_TOTAL_AMOUNT_COOKIE_NAME, totalAmount.ToString(), cookieOptions);
        }



        private bool RentalHistoryExists(int id)
        {
            return _context.RentalHistory.Any(e => e.RentalHistoryId == id);
        }

        private static IQueryable<RentalHistoryItemViewModel> Sort_Search(IQueryable<RentalHistoryItemViewModel> rentalHistories, decimal totalAmount)
        {
            // Фильтрация по totalAmount
            if (totalAmount > 0)
            {
                rentalHistories = rentalHistories.Where(o => o.TotalAmount == totalAmount);
            }

            return rentalHistories;
        }
    }
}
