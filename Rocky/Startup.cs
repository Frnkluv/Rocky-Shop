using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rocky.Data;
using Rocky.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Сервис для Создания контекста БД. В скобках какие опции я хочу настроить
            /* Итог: таким образом получена строка подключения и эта строка передана в options для SQL servera
             * Это  пока все, что было необходимо для добавления DbContext для исп-ия SQL сервера в проекте*/
            
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("DefualtConnection")));
            // НО пока sql еще не иммеет БД и таблицу, нужна миграция.
            // 1. ПИШУ: add-migration addCategoryToDatabase в package manager console
            // 2. Далее пишу: update-database      -> в mssql добавилась эта БД


            //подключения идентификации
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();
            //-> спускаюсь в UseEndpoints, добавляю RazorPages.     Позже Добавил IdentityRole и AddDefaultUI() для реализации ролей

            //регистрация сервиса mailjet в приложении
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddDistributedMemoryCache();
            //Добавление сессии!        (???для реализации корзины)
            services.AddHttpContextAccessor();
            services.AddSession(Options =>
            {
                Options.IdleTimeout = TimeSpan.FromMinutes(10);
                Options.Cookie.HttpOnly = true;
                Options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();    //добавление аутентификации. ОБЯЗАТЕЛЬНО перед авторизацией!
            //далее делаю миграцию и обновляю БД
            //делаю стр для входа, реги, ввода пароля и тд (страницы Razor):
            //щелкаю пкм по проекту Rocky -> Add -> New Scaffolded Item (Создать шаблонный элемент) -> выбираю Identity (удостоверение)
            //выбираю все страницы -> путь ~View\Shared\_Layout
            //в Endpoints подключаю RazorPages

            app.UseAuthorization();

            //Код для обработки сеансов/сессий. ПО для подключения сессий, уже будет готово к работе.
            app.UseSession();
            ///По дефолту код для обработки сессий может хранить только целые цисла или строки
            ///и если нужно сохранить списки целых чисел/объекто, то нет поддержки по в .NET Core
            ///Но можно добавить методы расширения для сессий, чтобы настроить такую возможность.
            /// -> Создаю папку куда добавлю методы расширения (в этой папке созд класс)


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();      //подключаю RazorPages
                //-> иду в лэйаут и добавляю рэйзорпайдж _LoginPartial

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
