using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CarRental_2.Controllers
{
    [Authorize]
    public class CarsController : Controller
    {
        private readonly int _pageSize;
        private readonly CarRental_2Context _context;
        private readonly ILogger<CarsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CAR_RENTAL_COST_COOKIE_NAME = "CarRentalCostPerDay";
        private const string CAR_SORT_ORDER_COOKIE_NAME = "CarSortOrder";

        public CarsController(CarRental_2Context context, UserManager<ApplicationUser> userManager, ILogger<CarsController> logger, IConfiguration appConfig = null)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;

            // Загрузка pageSize из конфигурации
            _pageSize = appConfig != null && int.TryParse(appConfig["Parameters:PageSize"], out int pageSizeConfig)
                ? pageSizeConfig
                : 20;
        }

        // GET: Cars
        public async Task<IActionResult> Index(decimal rentalCostPerDay, SortState sortOrder = SortState.RentalCostPerDayAsc, int page = 1)
        {
            // Проверка, аутентифицирован ли пользователь
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Определение эффективных значений фильтров: приоритет query-параметрам, иначе из куки
            decimal filterRentalCostPerDay = Request.Query.ContainsKey("RentalCostPerDay") ? rentalCostPerDay : GetRentalCostPerDayFromCookie();
            SortState filterSortOrder = Request.Query.ContainsKey("sortOrder") ? sortOrder : GetSortOrderFromCookie();

            // Получение всех автомобилей с классами
            var carsQuery = from car in _context.Cars
                            join carClass in _context.CarClasses
                            on car.CarClassId equals carClass.CarClassId
                            select new CarItemViewModel
                            {
                                CarId = car.CarId,
                                ClassName = carClass.Name,
                                Brand = car.Brand,
                                Model = car.Model,
                                LicensePlate = car.LicensePlate,
                                Year = car.Year,
                                RentalCostPerDay = car.RentalCostPerDay,
                                Status = car.Status
                            };

            // Сортировка и фильтрация с эффективными значениями
            carsQuery = Sort_Search(carsQuery, filterSortOrder, filterRentalCostPerDay);

            // Общее количество записей БЕЗ фильтров (для информации)
            var totalAll = await (from car in _context.Cars
                                  join carClass in _context.CarClasses
                                  on car.CarClassId equals carClass.CarClassId
                                  select car).CountAsync();

            // Разбиение на страницы
            var count = await carsQuery.CountAsync(); // Количество с фильтрами
            var pagedCars = await carsQuery.Skip((page - 1) * _pageSize).Take(_pageSize).ToListAsync();

            // Формирование модели для передачи представлению
            var csViewModel = new CarViewModel
            {
                Cars = pagedCars,
                PageViewModel = new PageViewModel(count, page, _pageSize),
                SortViewModel = new SortViewModel(filterSortOrder),
                RentalCostPerDay = filterRentalCostPerDay
            };

            // Информация для ViewBag (о фильтрах и куки)
            ViewBag.TotalAllRecords = totalAll;
            ViewBag.FilterRentalCostPerDay = filterRentalCostPerDay > 0 ? filterRentalCostPerDay.ToString() : "Все стоимости";
            ViewBag.FilterSortOrder = GetSortDescription(filterSortOrder);
            ViewBag.HasFilters = filterRentalCostPerDay > 0 || filterSortOrder != SortState.RentalCostPerDayAsc;
            ViewBag.ResetUrl = Url.Action("Index"); // Ссылка сброса (куки останутся, но RentalCostPerDay=0 обновит их)

            // Сохранение текущих фильтров в куки для следующего посещения
            SetFilterCookies(filterRentalCostPerDay, filterSortOrder);

            return View(csViewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarEditCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var car = new Car
            {
                CarClassId = vm.CarClassId,
                Brand = vm.Brand,
                Model = vm.Model,
                LicensePlate = vm.LicensePlate,
                Year = vm.Year,
                RentalCostPerDay = vm.RentalCostPerDay,
                Status = vm.Status
            };

            _context.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Cars/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            var vm = new CarEditCreateViewModel
            {
                CarId = car.CarId,
                CarClassId = car.CarClassId,
                Brand = car.Brand ?? string.Empty,
                Model = car.Model ?? string.Empty,
                LicensePlate = car.LicensePlate ?? string.Empty,
                Year = car.Year,
                RentalCostPerDay = car.RentalCostPerDay,
                Status = car.Status ?? string.Empty
            };

            //vm.CarClasses = new SelectList(await _context.CarClasses.ToListAsync(), "CarClassId", "Name", vm.CarClassId);
            return View(vm);
        }
        
        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarEditCreateViewModel vm)
        {
            if (id != vm.CarId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null) return NotFound();

            car.CarClassId = vm.CarClassId;
            car.Brand = vm.Brand;
            car.Model = vm.Model;
            car.LicensePlate = vm.LicensePlate;
            car.Year = vm.Year;
            car.RentalCostPerDay = vm.RentalCostPerDay;
            car.Status = vm.Status;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var car = await _context.Cars
                .SingleOrDefaultAsync(m => m.CarId == id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Cars/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _context.Cars.SingleOrDefaultAsync(m => m.CarId == id);
            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private string GetSortDescription(SortState sortOrder)
        {
            return sortOrder switch
            {
                SortState.RentalCostPerDayAsc => "По аренде/сутки (возр.)",
                SortState.RentalCostPerDayDesc => "По аренде/сутки (убыв.)",
                _ => "Без сортировки"
            };
        }

        private decimal GetRentalCostPerDayFromCookie()
        {
            if (Request.Cookies.TryGetValue(CAR_RENTAL_COST_COOKIE_NAME, out string value) && decimal.TryParse(value, out decimal cost))
            {
                return cost;
            }
            return 0m;
        }

        private SortState GetSortOrderFromCookie()
        {
            if (Request.Cookies.TryGetValue(CAR_SORT_ORDER_COOKIE_NAME, out string value) && Enum.TryParse<SortState>(value, true, out SortState sortOrder))
            {
                return sortOrder;
            }
            return SortState.RentalCostPerDayAsc;
        }

        private void SetFilterCookies(decimal rentalCostPerDay, SortState sortOrder)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = Request.Scheme == "https"
            };

            Response.Cookies.Append(CAR_RENTAL_COST_COOKIE_NAME, rentalCostPerDay.ToString(), cookieOptions);
            Response.Cookies.Append(CAR_SORT_ORDER_COOKIE_NAME, sortOrder.ToString(), cookieOptions);
        }



        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.CarId == id);
        }

        private static IQueryable<CarItemViewModel> Sort_Search(IQueryable<CarItemViewModel> cars, SortState sortOrder, decimal rentalCostPerDay)
        {
            switch (sortOrder)
            {
                case SortState.RentalCostPerDayAsc:
                    cars = cars.OrderBy(s => s.RentalCostPerDay);
                    break;
                case SortState.RentalCostPerDayDesc:
                    cars = cars.OrderByDescending(s => s.RentalCostPerDay);
                    break;
            }

            // Фильтрация по RentalCostPerDay
            if (rentalCostPerDay > 0)
            {
                cars = cars.Where(o => o.RentalCostPerDay == rentalCostPerDay);
            }

            return cars;
        }
    }
}
