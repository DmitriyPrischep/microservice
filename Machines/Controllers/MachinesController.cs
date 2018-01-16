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
using Machines.Models;
using NLog;
using System.Web.OData;
using System.Messaging;

namespace Machines.Controllers
{
    public class MachineInfController : ODataController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private MachinesContext db = new MachinesContext();

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [EnableQuery]
        public IQueryable<Machine> Get()
        {
            logger.Info("Request GET with OData");
            return db.Machines.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<Machine> Get([FromODataUri] int key)
        {
            logger.Info("Request GET with OData with ID = {0}", key);
            IQueryable<Machine> result = db.Machines.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }
    }


    public class MachinesController : ApiController
    {
        private IMachinesContext db = new MachinesContext();
        private ServiceContext dbService = new ServiceContext();

        // add these contructors
        public MachinesController()
        {
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

        private async void PutData(string data, RequestType recType)
        {
            ServiceContext Context = new ServiceContext();
            InputStatisticMessage Message = new InputStatisticMessage();
            Message.Detail = data;
            Message.RequestType = recType;
            Message.ServerName = ServerName.MACHINES;
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

        private async void SendStatistic()
        {
            using (HttpClient test = new HttpClient())
            {
                await test.GetAsync("http://localhost:6376/stat/start");
            }

            ServiceContext dbService = new ServiceContext();
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
                    await dbService.SaveChangesAsync();
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
                                if (msg.Label == "NON_MACHINES")
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
                                        var FindMessage = dbService.InputMessage.Find(msg.Message.Id);
                                        if (FindMessage != null)
                                        {
                                            dbService.InputMessage.Remove(FindMessage);
                                            try
                                            {
                                                await dbService.SaveChangesAsync();
                                            }
                                            catch (DbUpdateConcurrencyException ex)
                                            {
                                                ex.Entries.Single().Reload();
                                            }
                                        }
                                        break;
                                    case -1:
                                        var FindMsg = dbService.InputMessage.Find(msg.Message.Id);
                                        if (FindMsg != null)
                                        {
                                            logger.Error("Error in Statistic service. Deleted data from DataBase. Message:" + msg.Error);
                                            dbService.InputMessage.Remove(FindMsg);
                                            try
                                            {
                                                await dbService.SaveChangesAsync();
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

                            await dbService.SaveChangesAsync();
                        }

                        TimeSpan interval = new TimeSpan(0, 2, 30);
                        System.Threading.Thread.Sleep(interval);

                        InputMessage = dbService.InputMessage.ToList();
                        foreach (var temp in InputMessage)
                        {
                            if (temp.CountSendMessage < 3)
                            {
                                temp.CountSendMessage++;
                                queue.Send(temp);
                                try
                                {
                                    dbService.Entry(temp).State = EntityState.Modified;
                                    dbService.SaveChanges();
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

        public MachinesController(IMachinesContext context)
        {
            db = context;
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: api/Machines
        public IQueryable<Machine> GetMachines()
        {
            logger.Info("Request GET");
            return db.Machines;
        }

        // GET: api/Machines/5
        [ResponseType(typeof(Machine))]
        public async Task<IHttpActionResult> GetMachine(int id)
        {
            logger.Info("Request GET with ID = {0}", id);
            Machine machine = await db.Machines.FindAsync(id);
            if (machine == null)
            {
                logger.Error("ERROR request GET with ID = {0}. Not found ID", id);
                return NotFound();
            }
            logger.Info("Success request GET with ID = {0}", id);
            return Ok(machine);
        }

        // PUT: api/Machines/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMachine(int id, Machine machine)
        {
            await Task.Run(() => PutData("Put Machine", RequestType.PUT));
            logger.Info("Request PUT with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}", 
                id, machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
            if (!ModelState.IsValid)
            {
                logger.Warn("ABORTED request PUT with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}. Bad model.",
                id, machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
                return BadRequest(ModelState);
            }

            if (id != machine.Id)
            {
                logger.Warn("ABORTED request PUT with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}. Bad ID and data.ID.",
                id, machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
                return BadRequest();
            }

            //db.Entry(machine).State = EntityState.Modified;
            db.MarkAsModified(machine);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MachineExists(id))
                {
                    logger.Warn("ABORTED request PUT with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}. Not found ID.",
                        id, machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
                    return NotFound();
                }
                else
                {
                    logger.Warn("ABORTED request PUT with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}. BD Error.",
                        id, machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
                    return StatusCode(HttpStatusCode.InternalServerError);
                }
            }
            logger.Warn("Success request PUT with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}.",
                id, machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Machines
        [ResponseType(typeof(Machine))]
        public async Task<IHttpActionResult> PostMachine(Machine machine)
        {
            await Task.Run(() => PutData("Post Machine", RequestType.POST));
            logger.Info("Request POST with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}",
                machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
            if (!ModelState.IsValid)
            {
                logger.Info("ABORTED request POST with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}",
                machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
                return BadRequest(ModelState);
            }

            db.Machines.Add(machine);
            await db.SaveChangesAsync();
            logger.Info("Success request POST with ID = {0} Type = {1} Mark = {2} Model = {3} Year = {4} VIN = {2} StateNumber = {2} IdUser = {2}",
                machine.Type, machine.Mark, machine.Model, machine.Year, machine.VIN, machine.StateNumber, machine.IdUsers);
            return CreatedAtRoute("DefaultApi", new { id = machine.Id }, machine);
        }

        // DELETE: api/Machines/5
        [ResponseType(typeof(Machine))]
        public async Task<IHttpActionResult> DeleteMachine(int id)
        {
            await Task.Run(() => PutData("Delete Machine", RequestType.DELETE));
            logger.Info("Request DELETE with ID = {0}", id);
            Machine machine = await db.Machines.FindAsync(id);
            if (machine == null)
            {
                logger.Error("ERROR request DELETE with ID = {0}. Not found ID", id);
                return NotFound();
            }

            db.Machines.Remove(machine);
            await db.SaveChangesAsync();
            logger.Info("Success request DELETE with ID = {0}", id);
            return Ok(machine);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MachineExists(int id)
        {
            return db.Machines.Count(e => e.Id == id) > 0;
        }
    }
}