using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository
{
    public class OrderHeaderRepository:Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext context;

        public OrderHeaderRepository(ApplicationDbContext context): base(context)
        {
            this.context = context;
        }

        public void UpdateStatus(int id, string orderStatus, string paymentStatus=null)
        {
            var orderFromDb = context.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (paymentStatus!= null)
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = context.OrderHeaders.FirstOrDefault(x => x.Id ==id);
            orderFromDb.PaymentDate = DateTime.Now;
            orderFromDb.SessionId = sessionId;
            orderFromDb.PaymentIntentId = paymentIntentId;
        }

        public void Update(OrderHeader orderHeader)
        {
            context.OrderHeaders.Update(orderHeader);
        }


    }
}
