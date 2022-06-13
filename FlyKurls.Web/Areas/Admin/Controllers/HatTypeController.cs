using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using FlyKurls.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlyKurls.Web.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Administrator)]
    public class HatTypeController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public HatTypeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        // GET: HatTypeController
        public ActionResult Index()
        {
            var model = unitOfWork.HatType.GetAll();
            return View(model);
        }


        // GET: HatTypeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: HatTypeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(HatType hatType)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.HatType.Add(hatType);
                unitOfWork.Save();
                TempData["success"] = "Successfully added hat type!!!";

                return RedirectToAction(nameof(Index));
            }

            return View(hatType);
        }

        // GET: HatTypeController/Edit/5
        public ActionResult Edit(int? id)
        {
            var model = unitOfWork.HatType.GetFirstOrDefault(x => x.Id == id);

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

        // POST: HatTypeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HatType hatType)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.HatType.Update(hatType);
                unitOfWork.Save();
                TempData["success"] = "Successfully updated hat type!!";

                return RedirectToAction(nameof (Index));
            }

            return View(hatType);
        }

        // GET: HatTypeController/Delete/5
        public ActionResult Delete(int? id)
        {
            var model = unitOfWork.HatType.GetFirstOrDefault(x => x.Id==id);
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

        // POST: HatTypeController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePOST(int id)
        {
            var model = unitOfWork.HatType.GetFirstOrDefault(x =>x.Id==id);
            if (model == null)
            {
                return NotFound();
            }

            unitOfWork.HatType.Remove(model);
            unitOfWork.Save();
            TempData["success"] = "Successfully removed hat type!!!";
            return RedirectToAction(nameof(Index));
        }
    }
}
