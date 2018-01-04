using System;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Machines.Models
{
    public interface IMachinesContext : IDisposable
    {
        DbSet<Machine> Machines { get; }
        Task<int> SaveChangesAsync();
        void MarkAsModified(Machine item);
    }
}
