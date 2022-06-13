using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using FlyKurls.Models.ViewModels;
using FlyKurls.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace FlyKurls.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;


        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = unitOfWork.OrderDetail.GetAll(x => x.OrderId == orderId, includeProperties: "Product"),

            };

            return View(OrderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_PAY_NOW(OrderVM OrderVM)
        {
            OrderVM.OrderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == 
            OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");

            OrderVM.OrderDetails = unitOfWork.OrderDetail.GetAll(x => x.Id ==
            OrderVM.OrderHeader.Id, includeProperties: "Product");

            //Strip Settings
            var domain = "https://localhost:44341/";
            var options = new SessionCreateOptions
            {

                LineItems = new List<SessionLineItemOptions>()

                ,
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",

            };
            foreach (var item in OrderVM.OrderDetails)
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
            unitOfWork.OrderHeader.UpateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            var orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == orderHeaderid);
            if (orderHeader.PaymentStatus == StaticDetail.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                //Check the strip status
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, StaticDetail.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }

           
            return View(orderHeaderid);
        }

        [HttpPost]
        [Authorize(Roles = $"{StaticDetail.Role_Administrator}, {StaticDetail.Role_Employee}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails(OrderVM OrderVM)
        {
            var orderHeaderFromDb = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, tracked: false);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            unitOfWork.Save();
            TempData["success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), "Order", new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{StaticDetail.Role_Administrator}, {StaticDetail.Role_Employee}")]
        public IActionResult StartProcessing(OrderVM OrderVM)
        {
            var orderHeaderFromDb = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, tracked: false);
            unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, StaticDetail.StatusInProcess);
            unitOfWork.Save();
            TempData["success"] = "Order Processed Successfully.";
            return RedirectToAction(nameof(Details), "Order", new {orderId = OrderVM.OrderHeader.Id});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{StaticDetail.Role_Administrator}, {StaticDetail.Role_Employee}")]
        public IActionResult ShipOrder(OrderVM OrderVM)
        {
            var orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, tracked: false);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = StaticDetail.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if(orderHeader.PaymentStatus == StaticDetail.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            unitOfWork.OrderHeader.Update(orderHeader);
            unitOfWork.Save();
            TempData["success"] = "Order Shipped Successfully.";
            return RedirectToAction(nameof(Details), "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{StaticDetail.Role_Administrator}, {StaticDetail.Role_Employee}")]
        public IActionResult CancelOrder(OrderVM OrderVM)
        {
            var orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == OrderVM.OrderHeader.Id, tracked: false);
            if(orderHeader.PaymentStatus == StaticDetail.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId,
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetail.StatusCancelled, StaticDetail.StatusRefunded);

            }

            else
            {
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, StaticDetail.StatusCancelled, StaticDetail.StatusRefunded);

            }
            unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Details), "Order", new { orderId = OrderVM.OrderHeader.Id });
        }

        

        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(StaticDetail.Role_Administrator) || User.IsInRole(StaticDetail.Role_Employee))
            {
                orderHeaders = unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = unitOfWork.OrderHeader.GetAll(x => x.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");

            }

            switch (status)
            {
                case "paymentpending":
                    orderHeaders = orderHeaders.Where(x => x.PaymentStatus == StaticDetail.PaymentStatusPending);
                    break;

                case "completed":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == StaticDetail.StatusShipped);
                    break;

                case "inprocess":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == StaticDetail.StatusInProcess);
                    break;

                case "approved":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == StaticDetail.StatusApproved);
                    break;

                default:
                    break;
            }



            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
