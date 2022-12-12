using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rocky.Controllers
{
    [Authorize (Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        // для извлечения значений из БД
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;   // реализ "Зависимости". Для POST Upsert, чтоб картинка грузилась. + Добавляю это в конструктор ниже

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            // Для получения списка категорий из БД

            // Применяю EF Eager Loading, вместо закомм-го кода ниже в этом блоке
            IEnumerable<Product> objList = _db.Product.Include(u => u.Category).Include(u => u.ApplicationType);

            //IEnumerable<Product> objList = _db.Product;
            //foreach (var obj in objList)
            //{
            //    obj.Category = _db.Category.FirstOrDefault(x => x.Id == obj.CategoryId);
            //    obj.ApplicationType = _db.ApplicationType.FirstOrDefault(x => x.Id == obj.ApplicationTypeId);
            //}

            return View(objList);
        }




        // GET - UPSERT.    Upsert - метод создания и редактирования товара
        public IActionResult Upsert(int? id)
        {
            // Закомментированно тк ViewBag, ViewData, TempData - не строгая типизация. Взамен буду исп ViewModel
            /*
            // для выпадающегося списка !!
            IEnumerable<SelectListItem> CategoryDropDown = _db.Category.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            // передача от контроллера к представлению (во VIEW):
            ViewBag.CategoryDropDown = CategoryDropDown;

            Product product = new Product();
            */


            // НЕОБХОДИМЫ КОММЕНТЫ ПОЧЕМУ ТАК НАПИСАНО !!!

            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.Category.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),

                ApplicationTypeSelectList = _db.ApplicationType.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            if (id == null)
            {
                // this is for Create
                return View(productVM);
            }
            else
            {
                // update
                productVM.Product = _db.Product.Find(id);
                if (productVM.Product == null) return NotFound();

                return View(productVM);
            }
        }

        // POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public IActionResult Upsert(ProductVM productVM)
        {
            //1. В wwwroot создаю папку image, а в ней product. Там будут лежать картинки!
            // Затем, создаю статик класс WC в проекте, и в поле указываю путь к картинкам.
            // Следом, создаю privete readonly FIEL вверху проекта, для реализации паттерна зависимости.

            if (ModelState.IsValid)
            {
                // для получения загруженного изображения
                var files = HttpContext.Request.Form.Files;
                // путь к wwwroot
                var webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    // CREATING

                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();        //генерится новое рандомное имя
                    string extensions = Path.GetExtension(files[0].FileName);   //присваевание значенияе из загруженного файла

                    //копирую файл в новое место через upload, аплоад определяет это место
                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extensions), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    //обновляю ссылку на image, указывая новый путь для доступа
                    productVM.Product.Image = fileName + extensions;

                    //не указывается путь внутри конст, поэтому при каждом выводе изображеня нужно добавить ImagePath + имя внутри БД
                    //но можно этого не делать, добавив просто путь из WC.ImagePath ??


                    _db.Product.Add(productVM.Product);
                }
                //чтоб при редактировании картинка отображалась -> иду в представление Upsert 
                
                else
                {
                    // UPDATING !!!!
                    //логика такая: нужно получить объект из БД, удалить его, затем добавить новый

                    // AsNoTracking чтобы EF не отслеживал эту сущность, тк мешает апдейту(вылетает ошибка)
                    var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(x => x.Id == productVM.Product.Id);

                    //если true, то новый файл уже был получен для сущ-го продукта
                    if (files.Count() > 0)
                    {
                        //вставка кода из GET'a выше + надо реализовать удаление сущ файла 
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extensions = Path.GetExtension(files[0].FileName);

                        //удаление старого файла
                        var oldFile = Path.Combine(upload, objFromDb.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extensions), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }

                        //создание нового фото внутри папки на сервере
                        productVM.Product.Image = fileName + extensions;
                    }
                    else
                    {
                        //для изменений, если НЕ МЕНЯЛАСЬ КАРТИНКА
                        productVM.Product.Image = objFromDb.Image;
                    }

                    //обновления всех изменений
                    _db.Product.Update(productVM.Product);

                    //EF может отслеживать только ОДНУ сущность (в случае выше: productVM (мне надо)).
                    //Поэтому иду в objFromDb и откл ее трекинг, тк это 2-й объект отслеживания
                }

                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            
            //если модель не валидна(блок if = false, и выкл валид на странице), то при ошибке пропадает выпадающий список,
            // => определяем для productVM список категорий
            productVM.CategorySelectList = _db.Category.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            productVM.ApplicationTypeSelectList = _db.ApplicationType.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });

            return View(productVM);
        }




        // GET - DELETE
        public IActionResult Delete(int? id)       // int id - тк передается с помощью asp-route-id
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            //Product product = _db.Product.Find(id);
            //product.Category = _db.Category.Find(product.CategoryId);
            
            //EF Eager Loading (жадная/нетерпеливая загрузка). Эффективней кода выше
            Product product = _db.Product.Include(c => c.Category).Include(a => a.ApplicationType).FirstOrDefault(x => x.Id == id);


            if (product == null) return NotFound();

            return View(product);
        }

        // POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var obj = _db.Product.Find(id);
            if (obj == null) return NotFound();

            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;
            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            _db.Product.Remove(obj);
            _db.SaveChanges();
            
            return RedirectToAction("Index");
        }
    }
}
