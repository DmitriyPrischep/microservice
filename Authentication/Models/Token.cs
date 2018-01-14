using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Models
{
    public class Token
    {
        public int Id { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? TimeOfReleaseAccessToken { get; set; }
        public DateTime? TimeOfReleaseRefreshToken { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Code")]
        public int AccessCodeId { get; set; }
        public Code Code { get; set; }
    }

    public class Code
    {
        public int Id { get; set; }
        public string AccessCode { get; set; }
        public DateTime? Timeofrelease { get; set; }
        [ForeignKey("Owner")]
        public int OwnerId { get; set; }
        public Owner Owner { get; set; }
    }
    public class Owner
    {
        public int Id { get; set; }
        public int ClienSecret { get; set; }
        public string RedirectURI { get; set; }
        public string Name { get; set; }

    }

    public class TokenMessage
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}