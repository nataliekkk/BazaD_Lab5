using CarRental_2.Data;
using CarRental_2.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace CarRental_2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавление кэша для сессий
            builder.Services.AddDistributedMemoryCache();

            var services = builder.Services;

            //Вариант строки подключения к экземпляру локального SQL Server, не требующего секретной информации
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddDbContext<CarRental_2Context>(options => options.UseSqlServer(connectionString));

            // Настройка Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<CarRental_2Context>()
            .AddDefaultTokenProviders();

            //Добавление сессии
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".CarRental.Session";
                options.IdleTimeout = System.TimeSpan.FromSeconds(3600);
                options.Cookie.IsEssential = true;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews();
            services.AddRazorPages();

            var app = builder.Build();

            // Инициализация базы данных
            using (var serviceScope = app.Services.CreateScope())
            {
                // Обеспечиваем уникальность имени переменной
                var scopedServices = serviceScope.ServiceProvider;
                await InitializeAsync(scopedServices);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Clients/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // добавляем поддержку сессий
            app.UseSession();
            app.UseRouting();

            // использование Identity
            app.UseAuthentication();
            app.UseAuthorization();

            // добавляем компонента miidleware по инициализации базы данных
            app.UseDbInitializer();
            app.MapRazorPages();

            // устанавливаем сопоставление маршрутов с контроллерами и страницами
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
            name: "users",
            pattern: "Users/{action=Index}/{id?}",
            defaults: new { controller = "Users", action = "Index" });
            


            app.Run();
        }

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Создание администратора
            var adminUser = new ApplicationUser 
            { UserName = "vicogoss@mail.ru",
              Email = "vicogoss@mail.ru",
              RegistrationDate = DateTime.UtcNow
            };
            var userPassword = "123#ABc";
            var user = await userManager.FindByEmailAsync(adminUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(adminUser, userPassword);
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
