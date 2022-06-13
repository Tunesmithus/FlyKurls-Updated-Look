using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FlyKurls.DataAccess.Repository.IRepository
{
    public interface IRepository<T>
    {
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true);
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null);

        void Remove(T entity);
        void Add(T entity);
        void RemoveRange(IEnumerable<T> entity);

        

    }
}
