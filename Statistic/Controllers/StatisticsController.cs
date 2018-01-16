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
using Statistic.Models;
using System.Threading.Tasks;
using System.Messaging;

namespace Statistic.Controllers
{
    [RoutePrefix("stat")]
    public class StatisticsController : ApiController
    {
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
                        db2.stats.Add(statistic);
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
                                Msg.Label = "ANSAUTH";
                                break;
                            case ServerName.GATEWAY:
                                Msg.Label = "ANGATE";
                                break;
                            case ServerName.USERS:
                                Msg.Label = "ANCOMP";
                                break;
                            case ServerName.MACHINES:
                                Msg.Label = "ANPERS";
                                break;
                            case ServerName.FINES:
                                Msg.Label = "ANREG";
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
                            msg.Label = "ANSAUTH";
                            break;
                        case ServerName.GATEWAY:
                            msg.Label = "ANGATE";
                            break;
                        case ServerName.USERS:
                            msg.Label = "ANCOMP";
                            break;
                        case ServerName.MACHINES:
                            msg.Label = "ANPERS";
                            break;
                        case ServerName.FINES:
                            msg.Label = "ANREG";
                            break;
                    }
                    OutputQueue.Send(msg);
                }
            }

        }





























        private StatisticContext db = new StatisticContext();

        // GET: api/Statistics
        public IQueryable<Statistic> GetStatistics()
        {
            return db.Statistics;
        }

        // GET: api/Statistics/5
        [ResponseType(typeof(Statistic))]
        public IHttpActionResult GetStatistic(int id)
        {
            Statistic statistic = db.Statistics.Find(id);
            if (statistic == null)
            {
                return NotFound();
            }

            return Ok(statistic);
        }

        // PUT: api/Statistics/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutStatistic(int id, Statistic statistic)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != statistic.Id)
            {
                return BadRequest();
            }

            db.Entry(statistic).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatisticExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Statistics
        [ResponseType(typeof(Statistic))]
        public IHttpActionResult PostStatistic(Statistic statistic)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Statistics.Add(statistic);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = statistic.Id }, statistic);
        }

        // DELETE: api/Statistics/5
        [ResponseType(typeof(Statistic))]
        public IHttpActionResult DeleteStatistic(int id)
        {
            Statistic statistic = db.Statistics.Find(id);
            if (statistic == null)
            {
                return NotFound();
            }

            db.Statistics.Remove(statistic);
            db.SaveChanges();

            return Ok(statistic);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StatisticExists(int id)
        {
            return db.Statistics.Count(e => e.Id == id) > 0;
        }
    }
}