using FlyKurls.DataAccess.Repository.IRepository;
using FlyKurls.Models;

namespace FlyKurls.DataAccess.IRepository
{
    public interface IHatTypeRepository:IRepository<HatType>
    {
        void Update(HatType hatType);
    }
}