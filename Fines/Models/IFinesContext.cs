using System;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Fines.Models
{
    public interface IFinesContext : IDisposable
    {
        DbSet<Fine> Fines { get; }
        Task<int> SaveChangesAsync();
        void MarkAsModified(Fine item);
    }
}
