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

        // add these contructors
        public FinesController() { }

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