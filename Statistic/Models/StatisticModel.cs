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
        public int NonAuth { get; set; }
        public int Auth { get; set; }
        public List<int> StatTime { get; set; }
        public List<int> StatType { get; set; }
    }

    public class MicroserviceInformation
    {
        public List<int> StatTime { get; set; }
        public List<int> StatType { get; set; }
    }

    public class StatisticInformation
    {
        public MicroserviceInformation FinesInfo { get; set; }
        public MicroserviceInformation UsersInfo { get; set; }
        public MicroserviceInformation MachineInfo { get; set; }
        public GatewayInformation GateInfo { get; set; }
    }
}