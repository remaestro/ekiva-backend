using System;
using System.Threading.Tasks;
using Ekiva.Core.Entities;
using Ekiva.Core.Interfaces;
using Ekiva.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ekiva.Infrastructure.Repositories
{
    public class PolicyRepository : Repository<Policy>, IPolicyRepository
    {
        public PolicyRepository(EkivaDbContext context) : base(context)
        {
        }

        public async Task<Policy?> GetPolicyWithDetailsAsync(Guid id)
        {
            return await _context.Policies
                .Include(p => p.Risks)
                .ThenInclude(r => r.Covers)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
