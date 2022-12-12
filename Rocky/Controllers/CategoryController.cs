using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        // для извлечения значений из БД
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            // Для получения списка категорий из БД
            IEnumerable<Category> objList = _db.Category;
            
            return View(objList);
        }



        // GET - Create
        public IActionResult Create()
        {
            return View();
        }

        // POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]      // добавляется спец. токен защиты
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");   // перенаправляю в экшн индекс, после нажатия кнопки Create
            }

            return View(obj);
        }



        // GET - EDIT
        public IActionResult Edit(int? id)       // int id - тк передается с помощью asp-route-id
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Category.Find(id);
            if (obj == null) return NotFound();
            
            return View(obj);
        }

        // POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Update(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(obj);
        }



        // GET - DELETE
        public IActionResult Delete(int? id)       // int id - тк передается с помощью asp-route-id
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.Category.Find(id);
            if (obj == null) return NotFound();

            return View(obj);
        }

        // POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var obj = _db.Category.Find(id);
            if (obj == null) return NotFound();

            _db.Category.Remove(obj);
            _db.SaveChanges();
            
            return RedirectToAction("Index");
            
            // тут не так как на курсе сделал, но все работает
            //if (obj != null)
            //{
            //    _db.Category.Remove(obj);
            //    _db.SaveChanges();
            //    return RedirectToAction("Index");
            //}
            //return NotFound();
        }
    }
}
