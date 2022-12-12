using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rocky.Models;

namespace Rocky.Data
{
    //public class ApplicationDbContext : DbContext
    //IdentityDbContex - для реализации идентификации. После изм иду в StartUp, после строки подключения добавляю serviсes.AddDefaultIdentity
    public class ApplicationDbContext : IdentityDbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        // делаю св-во чтобы разместить в БД, в скобках сущность которую хочу создать в бд, далее имя - имя таблицы в БД
        // Итог: создана таблица в Category в БД
        // После иду в StartUp -> ConfigureServices -> добавляю сервис чтобы при запуске прил. запускалась БД (это вып. единожды в первый раз)
        public DbSet<Category> Category { get; set; }
        public DbSet<ApplicationType> ApplicationType { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }

    }
}
