using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Machines.Models
{
    public class Machine
    {
        public int Id { get; set; }
        public string Type { get; set; }
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Mark { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string VIN { get; set; }
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string StateNumber { get; set; }
        public int IdUsers { get; set; }
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