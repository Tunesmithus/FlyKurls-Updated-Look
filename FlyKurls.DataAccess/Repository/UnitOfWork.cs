using FlyKurls.DataAccess.IRepository;
using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            Category = new CategoryRepository(context);
            HatType = new HatTypeRepository(context);
            Product = new ProductRepository(context);
            Company = new CompanyRepository(context);
            ShoppingCart = new ShoppingCartRepository(context);
            ApplicationUser = new ApplicationUserRepository(context);
            OrderDetail = new OrderDetailRepository(context);
            OrderHeader = new OrderHeaderRepository(context);
            
        }
        public ICategoryRepository Category { get; private set; }

        public IHatTypeRepository HatType { get; private set; }
        public IProductRepository Product { get; private set; }

        public ICompanyRepository Company { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; set; }

        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }

       

        public void Save()
        {
           context.SaveChanges();
        }
    }
}
