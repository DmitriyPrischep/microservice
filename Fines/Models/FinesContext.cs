﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Fines.Models
{
    public class FinesContext : DbContext, IFinesContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public FinesContext() : base("name=FinesContext")
        {
        }

        public System.Data.Entity.DbSet<Fines.Models.Fine> Fines { get; set; }

        public void MarkAsModified(Fine item)
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

        public System.Data.Entity.DbSet<Fines.Models.InputStatisticMessage> InputMessage { get; set; }

    }
}
