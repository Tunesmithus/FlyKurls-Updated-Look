using FlyKurls.DataAccess;
using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlyKurls.Web.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        
        private readonly IUnitOfWork unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
           
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var model = unitOfWork.Category.GetAll();
            return View(model);
        }

        //Get: Category/Create
        [HttpGet]
        public IActionResult Create()
        {
            
            return View();
        }

        //Get: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id, Name, DisplayOrder")] Category category)
        {
            if (ModelState.IsValid != true)
            {
                return View(category);
            }

            if (category == null)
            {
                return View(category);
            }

            unitOfWork.Category.Add(category);
            unitOfWork.Save();
            TempData["success"] = "Category created successfully";
            return RedirectToAction(nameof(Index));
        }

        //Get: Edit/Category/4
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            var model = unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);

            if (id == null || id == 0)
            {
                return NotFound();
            }
           
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        //Get: Edit/Category/4
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid != true)
            {
                return View(category);
            }

            if (category == null)
            {
                return View(category);
            }

            unitOfWork.Category.Update(category);
            unitOfWork.Save();
            TempData["success"] = "Category updated successfully";

            return RedirectToAction(nameof(Index));
        }
        //Get: Edit/Category/4
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            var model = unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);

            if (id == null || id == 0)
            {
                return NotFound();
            }

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        //Get: Edit/Category/4
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var model = unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            unitOfWork.Category.Remove(model);
            unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";

            return RedirectToAction(nameof(Index));
        }




    }
}
