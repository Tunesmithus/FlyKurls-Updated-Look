using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using FlyKurls.Models.ViewModels;
using FlyKurls.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace FlyKurls.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IEmailSender emailSender;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int OrderTotal { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            this.unitOfWork = unitOfWork;
            this.emailSender = emailSender;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = unitOfWork.ApplicationUser.GetFirstOrDefault(
                x=> x.Id == claim.Value);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ListCart = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = StaticDetail.StatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.UtcNow;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }

            ApplicationUser applicationUser = unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetail.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetail.StatusApproved;
            }

            unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            unitOfWork.Save();

            foreach (var cart in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                unitOfWork.OrderDetail.Add(orderDetail);
                unitOfWork.Save();
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {

                //Strip Settings
                var domain = "https://localhost:44341/";
                var options = new SessionCreateOptions
                {

                    LineItems = new List<SessionLineItemOptions>()

                    ,
                    Mode = "payment",
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"customer/car/index",

                };
                foreach (var item in ShoppingCartVM.ListCart)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name,

                            },

                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                unitOfWork.OrderHeader.UpateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }
            else
            {
                return RedirectToAction(nameof(OrderConfirmation), "Cart", new {id = ShoppingCartVM.OrderHeader.Id} );
            }

            //unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            //unitOfWork.Save();
            //return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != StaticDetail.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //Check the strip status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStatus(id, StaticDetail.StatusApproved, StaticDetail.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }

            emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Fly Kurls", "<p>New Order Created></p>");
            List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId
            == orderHeader.ApplicationUserId).ToList();
            HttpContext.Session.Clear();
            unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            unitOfWork.Save();
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cart = unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            unitOfWork.ShoppingCart.IncrementCount(cart, 1);
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {

            var cart = unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (cart.Count <= 1)
            {
                unitOfWork.ShoppingCart.Remove(cart);
                var count = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
                HttpContext.Session.SetInt32(StaticDetail.SessionCart, count);
            }
            else
            {
                unitOfWork.ShoppingCart.DecrementCount(cart, 1);
            }
            unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveProduct(int cartId)
        {
            var cart = unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            unitOfWork.ShoppingCart.Remove(cart);
            unitOfWork.Save();
            var count = unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count - 1;
            HttpContext.Session.SetInt32(StaticDetail.SessionCart, count);
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity <= 50)
            {
                return price;
            }
            else
            {
                if(quantity <= 100)
                {
                    return price50;
                }
                return price100;
            }
        }
    }
}
