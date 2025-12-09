using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ekiva.Core.Common;

namespace Ekiva.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<IReadOnlyList<T>> GetAllAsync(); // Alias for ListAllAsync
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        IQueryable<T> GetQueryable();
    }
}
