using CarRental_2.Models;
using CarRental_2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CarRental_2.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class ClientsController : Controller
    {
        private readonly int _pageSize;
        private readonly CarRental_2Context _context;
        private readonly ILogger<ClientsController> _logger;
        private const string CLIENT_PHONE_COOKIE_NAME = "ClientPhoneNumber";
        private const string CLIENT_SORT_ORDER_COOKIE_NAME = "ClientSortOrder";

        public ClientsController(CarRental_2Context context, ILogger<ClientsController> logger, IConfiguration appConfig = null)
        {
            _context = context;
            _logger = logger;

            // Настройка pageSize из конфигурации
            _pageSize = appConfig != null && int.TryParse(appConfig["Parameters:PageSize"], out int pageSizeConfig)
                ? pageSizeConfig
                : 20;
        }

        // GET: Clients
        public async Task<IActionResult> Index(string PhoneNumber = "", SortState sortOrder = SortState.PhoneNumberAsc, int page = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Приоритет query → куки → дефолт
            string filterPhoneNumber = Request.Query.ContainsKey("PhoneNumber") ? PhoneNumber : GetPhoneNumberFromCookie();
            SortState filterSortOrder = Request.Query.ContainsKey("sortOrder") ? sortOrder : GetSortOrderFromCookie();

            // Запрос с сортировкой/фильтром
            IQueryable<Client> clientsQuery = _context.Clients;
            clientsQuery = Sort_Search(clientsQuery, filterSortOrder, filterPhoneNumber);

            // Статистика без фильтров
            var totalAll = await _context.Clients.CountAsync();

            // Пагинация (async)
            var count = await clientsQuery.CountAsync();
            var pagedClients = await clientsQuery
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Модель
            var clientsViewModel = new ClientsViewModel
            {
                Clients = pagedClients,
                PageViewModel = new PageViewModel(count, page, _pageSize),
                SortViewModel = new SortViewModel(filterSortOrder),
                PhoneNumber = filterPhoneNumber
            };

            // ViewBag для инфо-блока
            ViewBag.TotalAllRecords = totalAll;
            ViewBag.FilterPhoneNumber = string.IsNullOrEmpty(filterPhoneNumber) ? "Все телефоны" : $"\"{filterPhoneNumber}\"";
            ViewBag.FilterSortOrder = GetSortDescription(filterSortOrder);
            ViewBag.HasFilters = !string.IsNullOrEmpty(filterPhoneNumber) || filterSortOrder != SortState.PhoneNumberAsc;
            ViewBag.ResetUrl = Url.Action("Index");

            // Сохранение в куки
            SetFilterCookies(filterPhoneNumber, filterSortOrder);

            return View(clientsViewModel);
        }

        // GET: Clients/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName, LicenseNumber, PhoneNumber")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Перенаправление на метод Index после успешного добавления
            }

            // Если модель не валидна, возвращаем форму с сообщениями об ошибках
            return View(client);
        }

        // GET: Clients/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var client = await _context.Clients.SingleOrDefaultAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId, FullName, LicenseNumber, PhoneNumber")] Client client)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(client);
            }
            else
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.ClientId))
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

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .SingleOrDefaultAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.SingleOrDefaultAsync(m => m.ClientId == id);
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private string GetSortDescription(SortState sortOrder)
        {
            return sortOrder switch
            {
                SortState.PhoneNumberAsc => "По телефону (возр.)",
                SortState.PhoneNumberDesc => "По телефону (убыв.)",
                _ => "Без сортировки"
            };
        }

        private string GetPhoneNumberFromCookie()
        {
            if (Request.Cookies.TryGetValue(CLIENT_PHONE_COOKIE_NAME, out string value))
            {
                return value ?? "";
            }
            return "";
        }

        private SortState GetSortOrderFromCookie()
        {
            if (Request.Cookies.TryGetValue(CLIENT_SORT_ORDER_COOKIE_NAME, out string value) && Enum.TryParse<SortState>(value, true, out SortState sortOrder))
            {
                return sortOrder;
            }
            return SortState.PhoneNumberAsc;
        }

        private void SetFilterCookies(string phoneNumber, SortState sortOrder)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = Request.Scheme == "https"
            };

            Response.Cookies.Append(CLIENT_PHONE_COOKIE_NAME, phoneNumber, cookieOptions);
            Response.Cookies.Append(CLIENT_SORT_ORDER_COOKIE_NAME, sortOrder.ToString(), cookieOptions);
        }



        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }

        private static IQueryable<Client> Sort_Search(IQueryable<Client> clients, SortState sortOrder, string phoneNumber)
        {
            // Сортировка
            switch (sortOrder)
            {
                case SortState.PhoneNumberAsc:
                    clients = clients.OrderBy(s => s.PhoneNumber);
                    break;
                case SortState.PhoneNumberDesc:
                    clients = clients.OrderByDescending(s => s.PhoneNumber);
                    break;
                default:
                    break;
            }

            // Фильтрация по номеру телефона
            if (!string.IsNullOrEmpty(phoneNumber)) // Проверяем, что phoneNumber не пустой
            {
                clients = clients.Where(o => o.PhoneNumber != null && o.PhoneNumber.Contains(phoneNumber));
            }

            return clients;
        }
    }

}
