using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Users.Models
{
    public interface IUsersContext : IDisposable
    {
        DbSet<User> Users { get; }
        Task<int> SaveChangesAsync();
        void MarkAsModified(User item);
    }
}