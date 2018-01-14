using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Authentication.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
    }

    public class AuthModelID
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int ClientId { get; set; }
    }

    public class AuthModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthCodeModel
    {
        public string GrantType { get; set; }
        public string Code { get; set; }
        public string RedirectURI { get; set; }
        public int ClientId { get; set; }
    }

    public class AuthRoleModel
    {
        public string Token { get; set; }
        public string RequiredRole { get; set; }
    }
}