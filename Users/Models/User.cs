using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Users.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FIO { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public int IdFines { get; set; }
    }

    public enum RequestType
    {
        GET,
        PUT,
        POST,
        DELETE,
        LOGIN
    }

    public enum ServerName
    {
        GATEWAY,
        AUTHENTICATION,
        USERS,
        MACHINES,
        FINES
    }

    public class InputStatisticMessage
    {
        public int Id { get; set; }

        public ServerName ServerName { get; set; }

        public RequestType RequestType { get; set; }

        public Guid State { get; set; }

        public string Detail { get; set; }

        public DateTime Time { get; set; }
        public int CountSendMessage { get; set; }
    }

    public class OutputStatisticMessage
    {
        public int Status { get; set; }
        public string Error { get; set; }

        public InputStatisticMessage Message { get; set; }
    }
}