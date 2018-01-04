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

        // add these contructors
        public MachinesController() { }

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