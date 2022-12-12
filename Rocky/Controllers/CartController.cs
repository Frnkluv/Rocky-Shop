using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Utility;
using Rocky.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Rocky.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize]     // для получения к экшн методам нужно авторизоваться
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;   //доступ к wwwroot
        private readonly IEmailSender _emailSender;
        
        [BindProperty]      // атрибут, чтобы не указывать явно это св-во в экшенах. Доступно подефолту будет.
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }


        public IActionResult Index()
        {
            // нужно показать все товары в корзине
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            // проверка сессии 
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null 
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                // session exist (сущ-ет) => получаем все товары и отображаем их
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            // получить каждый товар из корзины
            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();

            // извлечение всех объектов продукт
            IEnumerable<Product> prodList = _db.Product.Where(w => prodInCart.Contains(w.Id));

            return View(prodList);
        }

        /* делаю представление этого метода, реализую кнопку удаления (метод Remove), далее
         * нужно создать View Model в папке models -> viewModels с именем ProductUserVM для дальнейшей 
         * реализации кнопки "континью" в корзине.
         * Далее нужно созд POST метод для Index чтоб он срабатывал принажатии кнопки класса submit
         */



        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));       // -> созд Summary
        }


        // GET
        public IActionResult Summary()
        {
            /* тут нужно показать имя, почту и ном. тел., которые вводил пользователь при регистрации
            * для этого нужно узнать id этого чела. Два способа ниже
            */

                // 1-ый
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                // 2-ой
            //var userId = User.FindFirstValue(ClaimTypes.Name);

            // доступ к корзине (вставка из Index):
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                // session exist (сущ-ет) => получаем все товары и отображаем их
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            // получить каждый товар из корзины
            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            IEnumerable<Product> prodList = _db.Product.Where(w => prodInCart.Contains(w.Id));

            // добавляю св-во ProductUserVM вверху класса, а реализация тут:
            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _db.ApplicationUser.FirstOrDefault(x => x.Id == claim.Value),
                ProductList = prodList.ToList()
            };

            return View(ProductUserVM);

            // добавляю View для метода
        }


        // POST
        [HttpPost, ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM ProductUserVM)
        {
            //Код для формирования и отправки письма на почту о выполненом заказе:

            //получение доступа к шаблону в темплейтс
            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

            var subject = "New Inquiry";
            string htmlBody = "";

            using(StreamReader sr = System.IO.File.OpenText(pathToTemplate))
            {
                htmlBody = sr.ReadToEnd();  // читает содержимое этого файла и сохр в боди
            }

            StringBuilder productListSB = new StringBuilder();
            foreach (var prod in ProductUserVM.ProductList)
            {
                productListSB.Append($" - Name: {prod.Name} <span style='font-size:14px;'> (Id: {prod.Id})</span><br />");
            }
            
            string messageBody = string.Format(htmlBody,
                ProductUserVM.ApplicationUser.FullName,
                ProductUserVM.ApplicationUser.Email,
                ProductUserVM.ApplicationUser.PhoneNumber,
                productListSB.ToString());
            // след шаг: добавить поле имэйлсендер

            await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);
            /* если клиент сделал заказ, то смс уйдет на почту EmailAdmin 
             */

            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
        }



        public IActionResult Remove(int id)
        {
            // извлечение всех товаров из корзины
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                // session exist (сущ-ет) => получаем все товары и отображаем их
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            
            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(x => x.ProductId == id));   // затем удаляем товар из корзины

            // устанавливаю снова значение для сессии с новым списком
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index));
        }
    }
}
