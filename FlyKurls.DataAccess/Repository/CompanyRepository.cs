using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext context;

        public CompanyRepository(ApplicationDbContext context):base(context)
        {
            this.context = context;
        }
        public void Update(Company company)
        {
            context.Companies.Update(company);
        }
    }
}
