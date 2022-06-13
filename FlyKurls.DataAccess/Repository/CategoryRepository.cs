using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository
{
    public class CategoryRepository:Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }


        public void Update(Category category)
        {
            context.Categories.Update(category);
        }
    }
}
