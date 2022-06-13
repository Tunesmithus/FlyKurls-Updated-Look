using FlyKurls.DataAccess.Repository;
using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using FlyKurls.Models.ViewModels;
using FlyKurls.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlyKurls.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Administrator)]
    public class ProductController : Controller
    {

        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;

        }

        // GET: ProductController
        public IActionResult Index()
        {
            
            return View();
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }



        // GET: ProductController/Edit/5
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new(),
                CategoryList = unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()

                }),

                HatTypeList = unitOfWork.HatType.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()

                }),

            };


            //Product product = new();
            //IEnumerable<SelectListItem> CategoryList = unitOfWork.Category.GetAll().Select(
            //    x => new SelectListItem
            //    {
            //        Text = x.Name,
            //        Value = x.Id.ToString()
            //    });

            //IEnumerable<SelectListItem> HatTypeList = unitOfWork.HatType.GetAll().Select(
            //    x => new SelectListItem
            //    {
            //        Text = x.Name,
            //        Value = x.Id.ToString()
            //    });

            if (id == null || id == 0)
            {
                //Creat Product
                //ViewBag.CategoryList = CategoryList;
                //ViewBag.HatTypeList = HatTypeList;
                return View(productVM);
            }
            else
            {
                //Update product
                productVM.Product = unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
                return View(productVM);

            }

            
        }



        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upsert(ProductVM productVM, IFormFile file)
        {

            if (ModelState.IsValid == true)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if (productVM.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using(var fileStreams = new FileStream(Path.Combine(uploads, fileName+extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }

                if (productVM.Product.Id == 0)
                {
                    unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    unitOfWork.Product.Update(productVM.Product);
                }
                unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));

            }
            return View(productVM);
           
        }

 

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var productlist = unitOfWork.Product.GetAll(includeProperties:"Category,HatType");
            return Json(new { data = productlist });
        }

        // POST: ProductController/Delete/5
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var product = unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Error while deleting"});
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            unitOfWork.Product.Remove(product);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });


        }
        #endregion
    }

   

}
