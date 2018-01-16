using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Statistic.Models;
using System.Threading.Tasks;
using System.Messaging;

namespace Statistic.Controllers
{
    [RoutePrefix("stat")]
    public class StatisticsController : ApiController
    {
        private StatisticContext db = new StatisticContext();

        public StatisticsController()
        {
            Task.Run(() => backwork());
        }

        private async void backwork()
        {
            StatisticContext db2 = new StatisticContext();
            MessageQueue InputQueue;
            if (MessageQueue.Exists(@".\private$\InputStatistic"))
            {
                InputQueue = new MessageQueue(@".\private$\InputStatistic");
            }
            else
            {
                InputQueue = MessageQueue.Create(".\\private$\\InputStatistic");
            }

            MessageQueue OutputQueue;
            if (MessageQueue.Exists(@".\private$\OutputStatistic"))
            {
                OutputQueue = new MessageQueue(@".\private$\OutputStatistic");
            }
            else
            {
                OutputQueue = MessageQueue.Create(".\\private$\\OutputStatistic");
            }

            using (InputQueue)
            {
                Statistic.Models.Statistic statistic = new Statistic.Models.Statistic();
                InputStatisticMessage InputMessage = new InputStatisticMessage();
                OutputStatisticMessage OutputMessage = new OutputStatisticMessage();
                Message msg = new Message();
                InputQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(InputStatisticMessage) });
                OutputQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(OutputStatisticMessage) });
                while (InputQueue.CanRead)
                {
                    Message msgInput = InputQueue.Receive();

                    InputMessage = (InputStatisticMessage)msgInput.Body;

                    statistic.RequestType = InputMessage.RequestType;
                    statistic.ServerName = InputMessage.ServerName;
                    statistic.Time = InputMessage.Time;
                    statistic.Detail = InputMessage.Detail;
                    statistic.State = InputMessage.State;

                    try
                    {
                        db2.Statistics.Add(statistic);
                        await db2.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        OutputStatisticMessage OutputMessageCatch = new OutputStatisticMessage();
                        OutputMessageCatch.Status = -1;
                        OutputMessageCatch.Error = "Error in Input Queue";
                        OutputMessageCatch.Message = InputMessage;

                        Message Msg = new Message(OutputMessageCatch);
                        switch (OutputMessageCatch.Message.ServerName)
                        {
                            case ServerName.AUTHENTICATION:
                                Msg.Label = "NON_AUTH";
                                break;
                            case ServerName.GATEWAY:
                                Msg.Label = "NON_GATEWAY";
                                break;
                            case ServerName.USERS:
                                Msg.Label = "NON_USERS";
                                break;
                            case ServerName.MACHINES:
                                Msg.Label = "NON_MACHINES";
                                break;
                            case ServerName.FINES:
                                Msg.Label = "NON_FINES";
                                break;
                        }
                        OutputQueue.Send(Msg);
                        continue;
                    }

                    OutputMessage.Status = 0;
                    OutputMessage.Error = "";
                    OutputMessage.Message = InputMessage;

                    msg.Body = OutputMessage;
                    switch (OutputMessage.Message.ServerName)
                    {
                        case ServerName.AUTHENTICATION:
                            msg.Label = "NON_AUTH";
                            break;
                        case ServerName.GATEWAY:
                            msg.Label = "NON_GATEWAY";
                            break;
                        case ServerName.USERS:
                            msg.Label = "NON_USERS";
                            break;
                        case ServerName.MACHINES:
                            msg.Label = "NON_MACHINES";
                            break;
                        case ServerName.FINES:
                            msg.Label = "NON_FINES";
                            break;
                    }
                    OutputQueue.Send(msg);
                }
            }
        }

        private GatewayInformation GetGatewayInformation(StatisticContext context)
        {
            var GateStatistic = context.Statistics.Where(x => x.ServerName == ServerName.GATEWAY).ToList();
            int CountUnauthorized = 0;
            int CountAccess = 0;
            List<int> arrayTime = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                arrayTime.Add(0);
            }
            List<int> arrayType = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                arrayType.Add(0);
            }

            foreach (var item in GateStatistic)
            {
                string q = item.Detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "UNAUTHORIZED")
                {
                    CountUnauthorized++;
                }
                if (w[0] == "ACCESS")
                {
                    CountAccess++;
                }

                arrayTime[item.Time.Value.Hour]++;
                arrayType[(int)item.RequestType]++;

            }
            GatewayInformation GateInfo = new GatewayInformation();
            GateInfo.NonAuth = CountUnauthorized;
            GateInfo.Auth = CountAccess;
            GateInfo.StatTime = arrayTime;
            GateInfo.StatType = arrayType;

            return GateInfo;
        }


        private MicroserviceInformation GetUsersInformation(StatisticContext context)
        {
            var UserStatistic = context.Statistics.Where(x => x.ServerName == ServerName.USERS).ToList();
            List<int> ArrayTime = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                ArrayTime.Add(0);
            }
            List<int> arrayType = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                arrayType.Add(0);
            }

            foreach (var item in UserStatistic)
            {
                string q = item.Detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "PUT")
                {
                    arrayType[0]++;
                }
                if (w[0] == "POST")
                {
                    arrayType[1]++;
                }
                if (w[0] == "DELETE")
                {
                    arrayType[2]++;
                }

                ArrayTime[item.Time.Value.Hour]++;
            }
            MicroserviceInformation Information = new MicroserviceInformation();
            Information.StatTime = ArrayTime;
            Information.StatType = arrayType;
            return Information;
        }

        private MicroserviceInformation GetMachineInformation(StatisticContext context)
        {
            var MachineStatistic = context.Statistics.Where(x => x.ServerName == ServerName.MACHINES).ToList();
            List<int> ArrayTime = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                ArrayTime.Add(0);
            }
            List<int> arrayType = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                arrayType.Add(0);
            }

            foreach (var item in MachineStatistic)
            {
                string q = item.Detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "PUT")
                {
                    arrayType[0]++;
                }
                if (w[0] == "POST")
                {
                    arrayType[1]++;
                }
                if (w[0] == "DELETE")
                {
                    arrayType[2]++;
                }
                ArrayTime[item.Time.Value.Hour]++;
            }
            MicroserviceInformation Information = new MicroserviceInformation();
            Information.StatTime = ArrayTime;
            Information.StatType = arrayType;
            return Information;
        }

        private MicroserviceInformation GetFinesInformation(StatisticContext context)
        {
            var FineStatistic = context.Statistics.Where(x => x.ServerName == ServerName.FINES).ToList();
            List<int> ArrayTime = new List<int>(24);
            for (int i = 0; i < 24; i++)
            {
                ArrayTime.Add(0);
            }
            List<int> arrayType = new List<int>(3);
            for (int i = 0; i < 3; i++)
            {
                arrayType.Add(0);
            }

            foreach (var item in FineStatistic)
            {
                string q = item.Detail.ToString();
                string[] w = q.Split(' ');
                if (w[0] == "PUT")
                {
                    arrayType[0] += 1;
                }
                if (w[0] == "POST")
                {
                    arrayType[1]++;
                }
                if (w[0] == "DELETE")
                {
                    arrayType[2]++;
                }
                ArrayTime[item.Time.Value.Hour]++;
            }
            MicroserviceInformation Information = new MicroserviceInformation();
            Information.StatTime = ArrayTime;
            Information.StatType = arrayType;
            return Information;
        }

        [Route("all")]
        public async Task<IHttpActionResult> GetAllInformation()
        {
            StatisticContext Context = new StatisticContext();
            StatisticInformation Information = new StatisticInformation();

            Information.GateInfo = GetGatewayInformation(Context);
            Information.FinesInfo = GetFinesInformation(Context);
            Information.UsersInfo = GetUsersInformation(Context);
            Information.MachineInfo = GetMachineInformation(Context);

            return Ok<StatisticInformation>(Information);
        }

        [Route("start")]
        public IHttpActionResult Get()
        {
            return Ok();
        }
    }
}