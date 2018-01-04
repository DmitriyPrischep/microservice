using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace UnitTestProject1
{
    public class TestUsersContextGood : IUsersContext
    {
        public TestUsersContextGood()
        {
            this.Users = new TestDBSetUsers();
        }

        public DbSet<User> Users { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return Task<int>.FromResult(0);
        }

        public void MarkAsModified(User item) { }
        public void Dispose() { }
    }

    public class TestUsersContextBad : IUsersContext
    {
        public TestUsersContextBad()
        {
            this.Users = new TestDBSetUsers();
        }

        public DbSet<User> Users { get; set; }

        public Task<int> SaveChangesAsync()
        {
            throw new DbUpdateConcurrencyException();
        }

        public void MarkAsModified(User item) { }
        public void Dispose() { }
    }
}
