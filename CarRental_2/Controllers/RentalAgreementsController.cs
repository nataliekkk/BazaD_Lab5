using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Drawing.Drawing2D;

namespace CarRental_2.Controllers
{
    [Authorize]
    public class RentalAgreementsController : Controller
    {
        private readonly int _pageSize;
        private readonly CarRental_2Context _context;
        private readonly ILogger<RentalAgreementsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string RENTAL_TOTAL_AMOUNT_COOKIE_NAME = "RentalTotalAmount";
        private const string RENTAL_SORT_ORDER_COOKIE_NAME = "RentalSortOrder";

        public RentalAgreementsController(CarRental_2Context context, UserManager<ApplicationUser> userManager, ILogger<RentalAgreementsController> logger, IConfiguration appConfig = null)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;

            // Загрузка pageSize из конфигурации
            _pageSize = appConfig != null && int.TryParse(appConfig["Parameters:PageSize"], out int pageSizeConfig)
                ? pageSizeConfig
                : 20;
        }

        // GET: RentalAgreements
        public async Task<IActionResult> Index(decimal TotalAmount, SortState sortOrder = SortState.No, int page = 1)
        {
            // Проверка, аутентифицирован ли пользователь
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Приоритет query → куки → дефолт
            decimal filterTotalAmount = Request.Query.ContainsKey("TotalAmount") ? TotalAmount : GetTotalAmountFromCookie();
            SortState filterSortOrder = Request.Query.ContainsKey("sortOrder") ? sortOrder : GetSortOrderFromCookie();

            // Получение всех договоров аренды (полный запрос БЕЗ фильтров для totalAll)
            var fullQuery = from agreement in _context.RentalAgreements
                            join client in _context.Clients on agreement.ClientId equals client.ClientId
                            join car in _context.Cars on agreement.CarId equals car.CarId
                            select new RentalAgreementItemViewModel
                            {
                                RentalAgreementId = agreement.RentalAgreementId,
                                FullName = client.FullName,
                                LicenseNumber = client.LicenseNumber,
                                PhoneNumber = client.PhoneNumber,
                                Brand = car.Brand,
                                Model = car.Model,
                                StartDateTime = agreement.StartDateTime,
                                PlannedEndDateTime = agreement.PlannedEndDateTime,
                                ActualEndDateTime = agreement.ActualEndDateTime,
                                TotalAmount = agreement.TotalAmount
                            };

            var totalAll = await fullQuery.CountAsync(); // Всего без фильтров

            // Фильтрованный запрос
            var rentalAgreementsQuery = fullQuery;
            rentalAgreementsQuery = Sort_Search(rentalAgreementsQuery, filterSortOrder, filterTotalAmount);

            // Разбиение на страницы
            var count = await rentalAgreementsQuery.CountAsync();
            var pagedRentalAgreements = await rentalAgreementsQuery.Skip((page - 1) * _pageSize).Take(_pageSize).ToListAsync();

            // Формирование модели для передачи представлению
            var rsViewModel = new RentalAgreementViewModel
            {
                RentalAgreements = pagedRentalAgreements,
                PageViewModel = new PageViewModel(count, page, _pageSize),
                SortViewModel = new SortViewModel(filterSortOrder),
                TotalAmount = filterTotalAmount
            };

            // ViewBag для инфо-блока
            ViewBag.TotalAllRecords = totalAll;
            ViewBag.FilterTotalAmount = filterTotalAmount > 0 ? filterTotalAmount.ToString() : "Все суммы";
            ViewBag.FilterSortOrder = GetSortDescription(filterSortOrder);
            ViewBag.HasFilters = filterTotalAmount > 0 || filterSortOrder != SortState.No;
            ViewBag.ResetUrl = Url.Action("Index");

            // Сохранение в куки
            SetFilterCookies(filterTotalAmount, filterSortOrder);

            return View(rsViewModel);
        }

        // GET: RentalAgreements/Create

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: RentalAgreements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalAgreementEditCreateViewModel ra)
        {
            if (!ModelState.IsValid)
            {
                return View(ra);
            }

            var rentalAgreement = new RentalAgreement
            {
                ClientId = ra.ClientId,
                CarId = ra.CarId,
                StartDateTime = ra.StartDateTime,
                PlannedEndDateTime = ra.PlannedEndDateTime,
                ActualEndDateTime = ra.ActualEndDateTime,
                TotalAmount = ra.TotalAmount
            };
            
            _context.Add(rentalAgreement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // Перенаправление на метод Index после успешного добавления
        }

        // GET: RentalAgreements/Edit/5

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var rentalAgreement = await _context.RentalAgreements.FindAsync(id);
            if (rentalAgreement == null)
            {
                return NotFound();
            }

            var ra = new RentalAgreementEditCreateViewModel
            {
                RentalAgreementId = rentalAgreement.RentalAgreementId,
                ClientId = rentalAgreement.ClientId,
                CarId = rentalAgreement.CarId,
                StartDateTime = rentalAgreement.StartDateTime,
                PlannedEndDateTime = rentalAgreement.PlannedEndDateTime,
                ActualEndDateTime = rentalAgreement.ActualEndDateTime,
                TotalAmount = rentalAgreement.TotalAmount
            };

            return View(ra);
        }

        // POST: RentalAgreements/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RentalAgreementEditCreateViewModel ra)
        {
            if (id != ra.RentalAgreementId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(ra);
            }
            else
            {
                try
                {
                    var rentalAgreement = await _context.RentalAgreements.FindAsync(id);
                    if (rentalAgreement == null) return NotFound();

                    rentalAgreement.ClientId = ra.ClientId;
                    rentalAgreement.CarId = ra.CarId;
                    rentalAgreement.StartDateTime = ra.StartDateTime;
                    rentalAgreement.PlannedEndDateTime = ra.PlannedEndDateTime;
                    rentalAgreement.ActualEndDateTime = ra.ActualEndDateTime;
                    rentalAgreement.TotalAmount = ra.TotalAmount;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalAgreementExists(ra.RentalAgreementId))
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

        // GET: RentalAgreements/Delete/5

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalAgreement = await _context.RentalAgreements
                .SingleOrDefaultAsync(m => m.RentalAgreementId == id);
            if (rentalAgreement == null)
            {
                return NotFound();
            }

            return View(rentalAgreement);
        }

        // POST: RentalAgreements/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rentalAgreement = await _context.RentalAgreements.SingleOrDefaultAsync(m => m.RentalAgreementId == id);
            _context.RentalAgreements.Remove(rentalAgreement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private string GetSortDescription(SortState sortOrder)
        {
            return sortOrder switch
            {
                SortState.TotalAmountAsc => "По сумме (возр.)",
                SortState.TotalAmountDesc => "По сумме (убыв.)",
                SortState.No => "Без сортировки",
                _ => "Без сортировки"
            };
        }

        private decimal GetTotalAmountFromCookie()
        {
            if (Request.Cookies.TryGetValue(RENTAL_TOTAL_AMOUNT_COOKIE_NAME, out string value) && decimal.TryParse(value, out decimal amount))
            {
                return amount;
            }
            return 0m;
        }

        private SortState GetSortOrderFromCookie()
        {
            if (Request.Cookies.TryGetValue(RENTAL_SORT_ORDER_COOKIE_NAME, out string value) && Enum.TryParse<SortState>(value, true, out SortState sortOrder))
            {
                return sortOrder;
            }
            return SortState.No;
        }

        private void SetFilterCookies(decimal totalAmount, SortState sortOrder)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = Request.Scheme == "https"
            };

            Response.Cookies.Append(RENTAL_TOTAL_AMOUNT_COOKIE_NAME, totalAmount.ToString(), cookieOptions);
            Response.Cookies.Append(RENTAL_SORT_ORDER_COOKIE_NAME, sortOrder.ToString(), cookieOptions);
        }


        private bool RentalAgreementExists(int id)
        {
            return _context.RentalAgreements.Any(e => e.RentalAgreementId == id);
        }

        private static IQueryable<RentalAgreementItemViewModel> Sort_Search(IQueryable<RentalAgreementItemViewModel> rentalAgreements, SortState sortOrder, decimal TotalAmount)
        {
            switch (sortOrder)
            {
                case SortState.TotalAmountAsc:
                    rentalAgreements = rentalAgreements.OrderBy(s => s.TotalAmount);
                    break;
                case SortState.TotalAmountDesc:
                    rentalAgreements = rentalAgreements.OrderByDescending(s => s.TotalAmount);
                    break;
            }

            // Фильтрация по totalAmount

            if (TotalAmount > 0)
            {
                rentalAgreements = rentalAgreements.Where(o => o.TotalAmount == TotalAmount);
            }

            return rentalAgreements;
        }
    }
}