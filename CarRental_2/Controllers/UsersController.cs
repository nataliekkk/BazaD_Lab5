using CarRental_2.Models;
using CarRental_2.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarRental_2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Получение всех пользователей
            var users = await _userManager.Users.ToListAsync(); // Извлечение всех пользователей асинхронно
            var userViewModel = new List<UserViewModel>(); // Инициализация списка

            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var urole = userRoles.FirstOrDefault() ?? ""; // Получение первой роли

                // Добавление пользователя в представление
                userViewModel.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    RegistrationDate = user.RegistrationDate,
                    RoleName = urole
                });
            }

            return View(userViewModel);
        }

        public IActionResult Create()
        {
            var allRoles = _roleManager.Roles.ToList();
            CreateUserViewModel user = new();

            ViewData["UserRole"] = new SelectList(allRoles, "Name", "Name");

            return View(user);

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new()
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    RegistrationDate = model.RegistrationDate,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Проверка роли и добавление
                    if (!string.IsNullOrEmpty(model.UserRole))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, model.UserRole);
                        if (!roleResult.Succeeded)
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            // Не возвращаем результат, чтобы снова показать форму с ошибками
                        }
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Повторно загружаем список ролей, если произошла ошибка
            ViewBag.UserRole = new SelectList(await _roleManager.Roles.ToListAsync(), "Name", "Name");
            return View(model); // Вернуть модель с ошибками валидации
        }

        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            // Получаем роли пользователя
            var userRoles = await _userManager.GetRolesAsync(user);
            string userRole = userRoles.FirstOrDefault() ?? ""; // Или получите вашу логику здесь

            // Загружаем все роли
            var allRoles = await _roleManager.Roles.ToListAsync();


            EditUserViewModel model = new()
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                RegistrationDate = user.RegistrationDate,
                UserRole = userRole
            };
            ViewData["UserRole"] = new SelectList(allRoles, "Name", "Name", model.UserRole);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    
                    var oldRoles = await _userManager.GetRolesAsync(user);

                    if (oldRoles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(user, oldRoles);

                    }
                    
                    var newRole = model.UserRole;
                    if (newRole.Length > 0)
                    {
                        await _userManager.AddToRoleAsync(user, newRole);
                    }
                    user.Email = model.Email;
                    user.UserName = model.UserName;
                    user.RegistrationDate = model.RegistrationDate;


                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }
    }
}