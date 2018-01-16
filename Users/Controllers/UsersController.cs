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
using Users.Models;
using System.Web.OData;
using NLog;
using System.Messaging;

namespace Users.Controllers
{
    public class UserInfController : ODataController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private UsersContext db = new UsersContext();
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [EnableQuery]
        public IQueryable<User> Get()
        {
            logger.Info("Request GET with OData");
            return db.Users.AsQueryable();
        }

        [EnableQuery]
        public SingleResult<User> Get([FromODataUri] int key)
        {
            logger.Info("Request GET with OData with ID = {0}", key);
            IQueryable<User> result =  db.Users.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }
    }


    public class UsersController : ApiController
    {
        //private UsersContext db = new UsersContext();
        private IUsersContext db = new UsersContext();
        private ServiceContext dbService = new ServiceContext();

        public UsersController()
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
            Message.ServerName = ServerName.USERS;
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
                                if (msg.Label == "NON_USERS")
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

        public UsersController(IUsersContext context)
        {
            db = context;
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();


        // GET: api/Users
        public IQueryable<User> GetUsers()
        {
            logger.Info("Request GET");
            return db.Users;
        }

        // GET: api/Users/5
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> GetUser(int id)
        {
            logger.Info("Request GET with ID = {0}", id);
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                logger.Error("ERROR request GET with ID = {0}. Not found ID", id);
                return NotFound();
            }
            logger.Info("Success request GET with ID = {0}", id);
            return Ok(user);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUser(int id, User user)
        {
            await Task.Run(() => PutData("Put User", RequestType.PUT));
            logger.Info("Request PUT with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}", id, user.FIO, user.Adress, user.Phone, user.IdFines);
            if (!ModelState.IsValid)
            {
                logger.Info("ABORTED request PUT with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}. Bad model.", id, user.FIO, user.Adress, user.Phone, user.IdFines);
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                logger.Info("ABORTED request PUT with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}. Bad ID and data.ID.", id, user.FIO, user.Adress, user.Phone, user.IdFines);
                return BadRequest();
            }

            //db.Entry(user).State = EntityState.Modified;
            db.MarkAsModified(user);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    logger.Info("ERROR request PUT with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}. Not Found ID.", id, user.FIO, user.Adress, user.Phone, user.IdFines);
                    return NotFound();
                }
                else
                {
                    logger.Info("ERROR request PUT with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}. BD error.", id, user.FIO, user.Adress, user.Phone, user.IdFines);
                    return StatusCode(HttpStatusCode.InternalServerError);
                }
            }
            logger.Info("Success request PUT with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}.", id, user.FIO, user.Adress, user.Phone, user.IdFines);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Users
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> PostUser(User user)
        {
            await Task.Run(() => PutData("Post User", RequestType.POST));
            logger.Info("Request POST with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}", user.FIO, user.Adress, user.Phone, user.IdFines);
            if (!ModelState.IsValid)
            {
                logger.Info("ABORTED POST with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}. Bad model.", user.FIO, user.Adress, user.Phone, user.IdFines);
                return BadRequest(ModelState);
            }
            db.Users.Add(user);

            await db.SaveChangesAsync();
            logger.Info("Success POST with ID = {0} FIO = {1} Adress = {2} Phone = {3} IdFines = {4}", user.FIO, user.Adress, user.Phone, user.IdFines);
            return CreatedAtRoute("DefaultApi", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            await Task.Run(() => PutData("Delete User", RequestType.DELETE));
            logger.Info("Request DELETE with ID = {0}", id);
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                logger.Error("Error request DELETE with ID = {0}. Not found ID", id);
                return NotFound();
            }

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            logger.Info("Success request DELETE with ID = {0}", id);
            return Ok(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }
    }
}