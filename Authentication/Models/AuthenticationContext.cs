using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Authentication.Models
{
    public class AuthenticationContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public AuthenticationContext() : base("name=AuthenticationContext")
        {
        }

        public System.Data.Entity.DbSet<Authentication.Models.User> Users { get; set; }

        public System.Data.Entity.DbSet<Authentication.Models.Token> Tokens { get; set; }

        public System.Data.Entity.DbSet<Authentication.Models.Code> Codes { get; set; }

        public System.Data.Entity.DbSet<Authentication.Models.Owner> Owners { get; set; }
    }
}
