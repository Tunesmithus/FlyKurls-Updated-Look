using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlyKurls.Web.ViewComponents
{
    public class ShoppingCartViewComponent: ViewComponent
    {
        private readonly IUnitOfWork unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if(HttpContext.Session.GetInt32(StaticDetail.SessionCart) != null)
                {
                    return View(HttpContext.Session.GetInt32(StaticDetail.SessionCart));
                }
                else
                {
                    HttpContext.Session.SetInt32(StaticDetail.SessionCart, unitOfWork.ShoppingCart.GetAll
                        (x => x.ApplicationUserId == claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(StaticDetail.SessionCart));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }

        }
    }
}
