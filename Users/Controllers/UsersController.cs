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

        public UsersController() { }

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
            logger.Info("Request DELETE with ID = {0}", id);
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                logger.Error("ERROR request DELETE with ID = {0}. Not found ID", id);
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