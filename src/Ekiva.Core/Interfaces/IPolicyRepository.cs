using System;
using System.Threading.Tasks;
using Ekiva.Core.Entities;

namespace Ekiva.Core.Interfaces
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<Policy?> GetPolicyWithDetailsAsync(Guid id);
    }
}
