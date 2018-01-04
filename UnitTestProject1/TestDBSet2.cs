using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machines.Models;
using Users.Models;
using Fines.Models;

namespace UnitTestProject1
{
    class TestDBSetMachines : TestDBSet<Machine>
    {
        public override Task<Machine> FindAsync(params object[] keyValues)
        {
            return Task<Machine>.FromResult(this.SingleOrDefault(product => product.Id == (int)keyValues.Single()));
        }
    }

    class TestDBSetUsers : TestDBSet<User>
    {
        public override Task<User> FindAsync(params object[] keyValues)
        {
            return Task<User>.FromResult(this.SingleOrDefault(product => product.Id == (int)keyValues.Single()));
        }
    }

    class TestDBSetFines : TestDBSet<Fine>
    {
        public override Task<Fine> FindAsync(params object[] keyValues)
        {
            return Task<Fine>.FromResult(this.SingleOrDefault(product => product.Id == (int)keyValues.Single()));
        }
    }
}
