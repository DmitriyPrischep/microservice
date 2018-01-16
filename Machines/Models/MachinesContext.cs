﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Machines.Models
{
    public class MachinesContext : DbContext, IMachinesContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public MachinesContext() : base("name=MachinesContext")
        {
        }

        public System.Data.Entity.DbSet<Machines.Models.Machine> Machines { get; set; }

        public void MarkAsModified(Machine item)
        {
            Entry(item).State = EntityState.Modified;
        }
    }

    public class ServiceContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public ServiceContext()
            : base("name=ServiceContext")
        {
        }

        public System.Data.Entity.DbSet<Machines.Models.InputStatisticMessage> InputMessage { get; set; }

    }
}
