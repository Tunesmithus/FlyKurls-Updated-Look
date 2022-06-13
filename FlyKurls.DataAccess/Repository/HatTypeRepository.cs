using FlyKurls.DataAccess.IRepository;
using FlyKurls.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository
{
    public class HatTypeRepository :Repository<HatType>, IHatTypeRepository
    {
        private readonly ApplicationDbContext context;

        public HatTypeRepository(ApplicationDbContext context):base(context)
        {
            this.context = context;
        }

        public void Update(HatType hatType)
        {
            context.HatTypes.Update(hatType);
        }
    }
}
