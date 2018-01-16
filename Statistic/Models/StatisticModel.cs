using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Statistic.Models
{
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

    public class Statistic
    {
        public int Id { get; set; }

        public ServerName ServerName { get; set; }

        public RequestType RequestType { get; set; }

        [Index(IsUnique = true)]
        public Guid State { get; set; }

        public string Detail { get; set; }

        public DateTime? Time { get; set; }
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

    public class GatewayInformation
    {
        public int Anauth { get; set; }
        public int Auth { get; set; }

        public List<int> rasp { get; set; }
        public List<int> resp2 { get; set; }

    }

    public class LittleInformation
    {
        public List<int> rasp { get; set; }
        public List<int> resp2 { get; set; }

    }

    public class StatisticInformation
    {
        public LittleInformation Information1 { get; set; }
        public LittleInformation Information2 { get; set; }
        public LittleInformation Information3 { get; set; }
        public GatewayInformation GateInfo { get; set; }
    }
}