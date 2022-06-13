using FlyKurls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository:IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);

        void UpdateStatus(int id, string orderStatus, string paymentStatus=null);
        void UpateStripePaymentID(int id, string sessionId, string paymentIntentId);

    }
}
