using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fines.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace UnitTestProject1
{
    public class TestFinesContextGood : IFinesContext
    {
        public TestFinesContextGood()
        {
            this.Fines = new TestDBSetFines();
        }

        public DbSet<Fine> Fines { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return Task<int>.FromResult(0);
        }

        public void MarkAsModified(Fine item) { }
        public void Dispose() { }
    }

    public class TestFinesContextBad : IFinesContext
    {
        public TestFinesContextBad()
        {
            this.Fines = new TestDBSetFines();
        }

        public DbSet<Fine> Fines { get; set; }

        public Task<int> SaveChangesAsync()
        {
            throw new DbUpdateConcurrencyException();
        }

        public void MarkAsModified(Fine item) { }
        public void Dispose() { }
    }
}
