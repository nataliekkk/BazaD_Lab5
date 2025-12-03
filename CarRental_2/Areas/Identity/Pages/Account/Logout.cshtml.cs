// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using CarRental_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CarRental_2.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Проверка на наличие пользовательского URL
            //if (IsUserOnAdminPage(returnUrl))
            //{
            //    // Показать сообщение, что выход невозможен с административной страницы
            //    return RedirectToPage("/Account/LogoutFailed");
            //}

            // Выполнение выхода
            await _signInManager.SignOutAsync();
            //_logger.LogInformation("User logged out.");

            // После выхода всегда перенаправляем на главную страницу
            return RedirectToPage("/Home"); // Измените на нужный путь вашей главной страницы

            //if (returnUrl != null)
            //{
            //    return LocalRedirect(returnUrl);
            //}
            //else
            //{
            //    return RedirectToPage();
            //}
        }

        private bool IsUserOnAdminPage(string returnUrl)
        {
            // Проверка, является ли возвращаемый URL страницей администратора
            return returnUrl?.Contains("/Users") == true;
        }
    }
}
