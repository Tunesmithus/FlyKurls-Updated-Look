using FlyKurls.DataAccess.Repository;
using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using FlyKurls.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlyKurls.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

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
            Company company = new();
            

            if (id == null || id == 0)
            {
                //Creat Product
                
                return View(company);
            }
            else
            {
                //Update product
                company = unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
                return View(company);

            }

            
        }



        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upsert(Company company)
        {

            if (ModelState.IsValid == true)
            {
                
                if (company.Id == 0)
                {
                    unitOfWork.Company.Add(company);
                    TempData["success"] = "Company added successfully";

                }

                else
                {
                    unitOfWork.Company.Update(company);
                    TempData["success"] = "Company updated successfully";

                }
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));

            }
            return View(company);
           
        }

        


        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }

        // POST: ProductController/Delete/5
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var company = unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);
            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting"});
            }

            unitOfWork.Company.Remove(company);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });


        }
        #endregion
    }

   

}
