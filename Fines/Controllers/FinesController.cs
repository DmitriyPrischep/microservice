using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Fines.Models;
using NLog;
using System.Web.OData;
using System.Messaging;

namespace Fines.Controllers
{
    public class FineInfController : ODataController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private FinesContext db = new FinesContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [EnableQuery]
        public IQueryable<Fine> Get()
        {
            logger.Info("Request GET with OData");
            return db.Fines.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<Fine> Get([FromODataUri] int key)
        {
            logger.Info("Request GET with OData with ID = {0}", key);
            IQueryable<Fine> result = db.Fines.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }
    }

    public class FinesController : ApiController
    {
        private IFinesContext db = new FinesContext();
        private ServiceContext dbService = new ServiceContext();

        // add these contructors
        public FinesController() {
            Task.Run(() => SendStatistic());
        }

        private async void Configure(ServiceContext Context)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            MessageQueue queue;
            if (MessageQueue.Exists(@".\private$\OutputStatistic"))
            {
                queue = new MessageQueue(@".\private$\OutputStatistic");
            }
            else
            {
                queue = MessageQueue.Create(".\\private$\\OutputStatistic");
            }

            using (queue)
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(OutputStatisticMessage) });
                Message[] Messages = queue.GetAllMessages();
                List<OutputStatisticMessage> OutputMessages = new List<OutputStatisticMessage>();

                foreach (var msg in Messages)
                {
                    if (msg.Label == "NON_USERS")
                    {
                        queue.ReceiveById(msg.Id);
                        OutputMessages.Add((OutputStatisticMessage)msg.Body);
                    }
                }

                foreach (var msg in OutputMessages)
                {
                    switch (msg.Status)
                    {
                        case 0:
                            var FindMsg = Context.InputMessage.Find(msg.Message.Id);
                            if (FindMsg != null)
                            {
                                Context.InputMessage.Remove(FindMsg);
                                try
                                {
                                    await Context.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    ex.Entries.Single().Reload();
                                }
                            }
                            break;
                        case -1:
                            var FindMsg1 = Context.InputMessage.Find(msg.Message.Id);
                            if (FindMsg1 != null)
                            {
                                logger.Error("Error in Statistic. Deleted data from database. Message:" + msg.Error);
                                Context.InputMessage.Remove(FindMsg1);
                                try
                                {
                                    await Context.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException ex)
                                {
                                    ex.Entries.Single().Reload();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                await Context.SaveChangesAsync();
                return;
            }
        }

        private async void SendStatistic()
        {
            using (HttpClient test = new HttpClient())
            {
                await test.GetAsync("http://localhost:6376/stat/start");
            }

            ServiceContext db = new ServiceContext();
            MessageQueue queue;
            if (MessageQueue.Exists(@".\private$\InputStatistic"))
            {
                queue = new MessageQueue(@".\private$\InputStatistic");
            }
            else
            {
                queue = MessageQueue.Create(".\\private$\\InputStatistic");
            }


            MessageQueue queueStat;
            if (MessageQueue.Exists(@".\private$\OutputStatistic"))
            {
                queueStat = new MessageQueue(@".\private$\OutputStatistic");
            }
            else
            {
                queueStat = MessageQueue.Create(".\\private$\\OutputStatistic");
            }

            using (queue)
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(InputStatisticMessage) });
                while (true)
                {
                    await db.SaveChangesAsync();
                    List<InputStatisticMessage> InputMessage = new List<InputStatisticMessage>();
                    try
                    {
                        using (queueStat)
                        {
                            queueStat.Formatter = new XmlMessageFormatter(new Type[] { typeof(OutputStatisticMessage) });

                            Message[] message = queueStat.GetAllMessages();

                            List<string> msgsuid = new List<string>();
                            List<OutputStatisticMessage> StatisticMessage = new List<OutputStatisticMessage>();

                            foreach (var msg in message)
                            {
                                if (msg.Label == "NON_FINES")
                                {
                                    queueStat.ReceiveById(msg.Id);
                                    StatisticMessage.Add((OutputStatisticMessage)msg.Body);
                                }
                            }

                            foreach (var msg in StatisticMessage)
                            {
                                switch (msg.Status)
                                {
                                    case 0:
                                        var FindMessage = db.InputMessage.Find(msg.Message.Id);
                                        if (FindMessage != null)
                                        {
                                            db.InputMessage.Remove(FindMessage);
                                            try
                                            {
                                                await db.SaveChangesAsync();
                                            }
                                            catch (DbUpdateConcurrencyException ex)
                                            {
                                                ex.Entries.Single().Reload();
                                            }
                                        }
                                        break;
                                    case -1:
                                        var FindMsg = db.InputMessage.Find(msg.Message.Id);
                                        if (FindMsg != null)
                                        {
                                            logger.Error("Error in Statistic service. Deleted data from DataBase. Message:" + msg.Error);
                                            db.InputMessage.Remove(FindMsg);
                                            try
                                            {
                                                await db.SaveChangesAsync();
                                            }
                                            catch (DbUpdateConcurrencyException ex)
                                            {
                                                ex.Entries.Single().Reload();
                                            }

                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }

                            await db.SaveChangesAsync();
                        }

                        TimeSpan interval = new TimeSpan(0, 2, 30);
                        System.Threading.Thread.Sleep(interval);

                        InputMessage = db.InputMessage.ToList();
                        foreach (var temp in InputMessage)
                        {
                            if (temp.CountSendMessage < 3)
                            {
                                temp.CountSendMessage++;
                                queue.Send(temp);
                                try
                                {
                                    db.Entry(temp).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    catch
                    {
                        TimeSpan interval = new TimeSpan(0, 2, 0);
                        System.Threading.Thread.Sleep(interval);
                    }
                }
            }
        }

        private async void PutData(string data, RequestType recType)
        {
            ServiceContext Context = new ServiceContext();
            InputStatisticMessage Message = new InputStatisticMessage();
            Message.Detail = data;
            Message.RequestType = recType;
            Message.ServerName = ServerName.FINES;
            Message.Time = DateTime.Now;
            Message.State = Guid.NewGuid();

            try
            {
                Context.InputMessage.Add(Message);
                await Context.SaveChangesAsync();
            }
            catch
            {
                return;
            }
        }

        public FinesController(IFinesContext context)
        {
            db = context;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();


        // GET: api/Fines
        public IQueryable<Fine> GetFines()
        {
            logger.Info("Request GET");
            return db.Fines;
        }

        // GET: api/Fines/5
        [ResponseType(typeof(Fine))]
        public async Task<IHttpActionResult> GetFine(int id)
        {
            logger.Info("Request GET with ID = {0}", id);
            Fine fine = await db.Fines.FindAsync(id);
            if (fine == null)
            {
                logger.Error("ERROR request GET with ID = {0}. Not found ID", id);
                return NotFound();
            }
            logger.Info("Success request GET with ID = {0}", id);
            return Ok(fine);
        }

        // PUT: api/Fines/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutFine(int id, Fine fine)
        {
            await Task.Run(() => PutData("Put Fine", RequestType.PUT));
            logger.Info("Request PUT with ID = {0} NameFine = {1} AmountFine = {2}", id, fine.NameFine, fine.AmountFine);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with ID = {0} NameFine = {1} AmountFine = {2}. Bad model.", id, fine.NameFine, fine.AmountFine);
                return BadRequest(ModelState);
            }

            if (id != fine.Id)
            {
                logger.Warn("ABORTED request PUT with ID = {0} NameFine = {1} AmountFine = {2}. Bad ID and data.ID.", id, fine.NameFine, fine.AmountFine);
                return BadRequest();
            }

            //db.Entry(fine).State = EntityState.Modified;
            db.MarkAsModified(fine);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FineExists(id))
                {
                    logger.Warn("ABORTED request PUT with ID = {0} NameFine = {1} AmountFine = {2}.  Not found ID", id, fine.NameFine, fine.AmountFine);
                    return NotFound();
                }
                else
                {
                    logger.Warn("ABORTED request PUT with ID = {0} NameFine = {1} AmountFine = {2}.  BD Error", id, fine.NameFine, fine.AmountFine);
                    return StatusCode(HttpStatusCode.InternalServerError);
                }
            }
            logger.Warn("Success request PUT with ID = {0} NameFine = {1} AmountFine = {2}.", id, fine.NameFine, fine.AmountFine);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Fines
        [ResponseType(typeof(Fine))]
        public async Task<IHttpActionResult> PostFine(Fine fine)
        {
            await Task.Run(() => PutData("Post Fine", RequestType.POST));
            logger.Info("Request POST with ID = {0} NameFine = {1} AmountFine = {2}", fine.NameFine, fine.AmountFine);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request POST with ID = {0} NameFine = {1} AmountFine = {2}.  Not found ID", fine.NameFine, fine.AmountFine);
                return BadRequest(ModelState);
            }

            db.Fines.Add(fine);
            await db.SaveChangesAsync();
            logger.Warn("Success request POST with ID = {0} NameFine = {1} AmountFine = {2}.  Not found ID", fine.NameFine, fine.AmountFine);
            return CreatedAtRoute("DefaultApi", new { id = fine.Id }, fine);
        }

        // DELETE: api/Fines/5
        [ResponseType(typeof(Fine))]
        public async Task<IHttpActionResult> DeleteFine(int id)
        {
            await Task.Run(() => PutData("Delete Fine", RequestType.DELETE));
            logger.Info("Request DELETE with ID = {0}", id);
            Fine fine = await db.Fines.FindAsync(id);
            if (fine == null)
            {
                logger.Error("ERROR request DELETE with ID = {0}. Not found ID", id);
                return NotFound();
            }

            db.Fines.Remove(fine);
            await db.SaveChangesAsync();
            logger.Info("Success request DELETE with ID = {0}", id);
            return Ok(fine);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FineExists(int id)
        {
            return db.Fines.Count(e => e.Id == id) > 0;
        }
    }
}