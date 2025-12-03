using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CarRental_2.Controllers
{

    [Authorize]
    public class MaintenancesController : Controller
    {
        private readonly int _pageSize;
        private readonly CarRental_2Context _context;
        private readonly ILogger<MaintenancesController> _logger;
        private const string COST_COOKIE_NAME = "MaintenanceCost";
        private const string SORT_ORDER_COOKIE_NAME = "MaintenanceSortOrder";

        public MaintenancesController(CarRental_2Context context, ILogger<MaintenancesController> logger, IConfiguration appConfig = null)
        {
            _context = context;
            _logger = logger;

            // Загрузка pageSize из конфигурации
            _pageSize = appConfig != null && int.TryParse(appConfig["Parameters:PageSize"], out int pageSizeConfig)
                ? pageSizeConfig
                : 20;
        }

        // GET: Maintenance
        public async Task<IActionResult> Index(decimal Cost, SortState sortOrder = SortState.CostAsc, int page = 1)
        {
            // Проверка, аутентифицирован ли пользователь
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Определение эффективных значений фильтров: приоритет query-параметрам, иначе из куки
            decimal filterCost = Request.Query.ContainsKey("Cost") ? Cost : GetCostFromCookie();
            SortState filterSortOrder = Request.Query.ContainsKey("sortOrder") ? sortOrder : GetSortOrderFromCookie();

            // Получение всех автомобилей с классами
            var MaintenanceQuery = from maintenance in _context.Maintenances
                                   join car in _context.Cars on maintenance.CarId equals car.CarId
                                   select new MaintenanceItemViewModel
                                   {
                                       Id = maintenance.Id,
                                       Brand = car.Brand,
                                       Model = car.Model,
                                       MaintenanceDate = maintenance.MaintenanceDate,
                                       Description = maintenance.Description,
                                       Cost = maintenance.Cost
                                   };

            // Сортировка и фильтрация с эффективными значениями
            MaintenanceQuery = Sort_Search(MaintenanceQuery, filterSortOrder, filterCost);

            // Общее количество записей БЕЗ фильтров (для информации)
            var totalAll = await (from m in _context.Maintenances
                                  join c in _context.Cars on m.CarId equals c.CarId
                                  select m).CountAsync();

            // Разбиение на страницы
            var count = await MaintenanceQuery.CountAsync(); // Количество с фильтрами
            var pagedMaintenance = await MaintenanceQuery.Skip((page - 1) * _pageSize).Take(_pageSize).ToListAsync();

            // Формирование модели для передачи представлению
            var msViewModel = new MaintenanceViewModel
            {
                Maintenances = pagedMaintenance,
                PageViewModel = new PageViewModel(count, page, _pageSize),
                SortViewModel = new SortViewModel(filterSortOrder),
                Cost = filterCost
            };

            // Информация для ViewBag (о фильтрах и куки)
            ViewBag.TotalAllRecords = totalAll;
            ViewBag.FilterCost = filterCost > 0 ? filterCost.ToString() : "Все стоимости";
            ViewBag.FilterSortOrder = GetSortDescription(filterSortOrder);
            ViewBag.HasFilters = filterCost > 0 || filterSortOrder != SortState.CostAsc;
            ViewBag.ResetUrl = Url.Action("Index"); // Ссылка сброса (куки останутся, но Cost=0 обновит их)

            // Сохранение текущих фильтров в куки для следующего посещения
            SetFilterCookies(filterCost, filterSortOrder);

            return View(msViewModel);
        }


        // GET: Maintenance/Create

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceEditCreateViewModel mm)
        {
            if (!ModelState.IsValid)
            {
                // Если модель не валидна, возвращаем форму с сообщениями об ошибках
                return View(mm);
            }
            var maintenance = new Maintenance
            {
                CarId = mm.CarId,
                MaintenanceDate = mm.MaintenanceDate,
                Description = mm.Description,
                Cost = mm.Cost
            };

            _context.Add(maintenance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Перенаправление на метод Index после успешного добавления

        }

        // GET: Maintenance/Edit/5

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }

            var mm = new MaintenanceEditCreateViewModel
            {
                Id = maintenance.Id,
                CarId = maintenance.CarId,
                MaintenanceDate = maintenance.MaintenanceDate,
                Description = maintenance.Description,
                Cost = maintenance.Cost
            };
            return View(mm);
        }

        // POST: Maintenance/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceEditCreateViewModel mm)
        {
            if (id != mm.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(mm);
            }
            else
            {
                try
                {
                    var maintenance = await _context.Maintenances.FindAsync(id);
                    if (maintenance == null) return NotFound();

                    maintenance.CarId = mm.CarId;
                    maintenance.MaintenanceDate = mm.MaintenanceDate;
                    maintenance.Description = mm.Description;
                    maintenance.Cost = mm.Cost;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceExists(mm.Id))
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

        // GET: Maintenance/Delete/5

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _context.Maintenances
                .SingleOrDefaultAsync(m => m.Id == id);
            if (maintenance == null)
            {
                return NotFound();
            }

            return View(maintenance);
        }

        // POST: Maintenance/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var maintenance = await _context.Maintenances.SingleOrDefaultAsync(m => m.Id == id);

            if (maintenance == null)
            {
                return NotFound();
            }

            _context.Maintenances.Remove(maintenance);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        private string GetSortDescription(SortState sortOrder)
        {
            return sortOrder switch
            {
                SortState.CostAsc => "По стоимости (возр.)",
                SortState.CostDesc => "По стоимости (убыв.)",
                _ => "Без сортировки"
            };
        }

        private decimal GetCostFromCookie()
        {
            if (Request.Cookies.TryGetValue(COST_COOKIE_NAME, out string value) && decimal.TryParse(value, out decimal cost))
            {
                return cost;
            }
            return 0m;
        }

        private SortState GetSortOrderFromCookie()
        {
            if (Request.Cookies.TryGetValue(SORT_ORDER_COOKIE_NAME, out string value) && Enum.TryParse<SortState>(value, true, out SortState sortOrder))
            {
                return sortOrder;
            }
            return SortState.CostAsc;
        }

        private void SetFilterCookies(decimal cost, SortState sortOrder)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = Request.Scheme == "https"
            };

            Response.Cookies.Append(COST_COOKIE_NAME, cost.ToString(), cookieOptions);
            Response.Cookies.Append(SORT_ORDER_COOKIE_NAME, sortOrder.ToString(), cookieOptions);
        }


        private bool MaintenanceExists(int id)
        {
            return _context.Maintenances.Any(e => e.Id == id);
        }

        private static IQueryable<MaintenanceItemViewModel> Sort_Search(IQueryable<MaintenanceItemViewModel> maintenance, SortState sortOrder, decimal Cost)
        {
            switch (sortOrder)
            {
                case SortState.CostAsc:
                    maintenance = maintenance.OrderBy(s => s.Cost);
                    break;
                case SortState.CostDesc:
                    maintenance = maintenance.OrderByDescending(s => s.Cost);
                    break;
            }

            // Фильтрация по Cost
            if (Cost > 0)
            {
                maintenance = maintenance.Where(o => o.Cost == Cost);
            }

            return maintenance;
        }
    }
}