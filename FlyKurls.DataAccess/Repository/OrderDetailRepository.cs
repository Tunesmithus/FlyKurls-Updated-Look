using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext context;

        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public void Update(OrderDetail orderDetail)
        {
            context.OrderDetails.Update(orderDetail);
        }
    }
}
