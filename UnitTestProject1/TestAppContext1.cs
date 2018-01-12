using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Machines.Models;

namespace UnitTestProject1
{
    public class TestMachineContextGood : IMachinesContext
    {
        public TestMachineContextGood()
        {
            this.Machines = new TestDBSetMachines();
        }

        public DbSet<Machine> Machines { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return Task<int>.FromResult(0);
        }

        public void MarkAsModified(Machine item) { }
        public void Dispose() { }
    }

    public class TestMachineContextBad : IMachinesContext
    {
        public TestMachineContextBad()
        {
            this.Machines = new TestDBSetMachines();
        }

        public DbSet<Machine> Machines { get; set; }

        public Task<int> SaveChangesAsync()
        {
            throw new DbUpdateConcurrencyException();
        }

        public void MarkAsModified(Machine item) { }
        public void Dispose() { }
    }
}
