using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Machines.Models;
using Machines.Controllers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Net;
using System.Web.Http;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestMachine
    {
        [TestMethod]
        public void PostProduct_ShouldReturnSameProduct()
        {
            var controller = new MachinesController(new TestMachineContextGood());

            var item = GetDemoProduct();

            var result =
                controller.PostMachine(item).Result as CreatedAtRouteNegotiatedContentResult<Machine>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RouteName, "DefaultApi");
            Assert.AreEqual(result.RouteValues["id"], result.Content.Id);
            Assert.AreEqual(result.Content.Type, item.Type);
            Assert.AreEqual(result.Content.Mark, item.Mark);
            Assert.AreEqual(result.Content.Model, item.Model);
            Assert.AreEqual(result.Content.Year, item.Year);
            Assert.AreEqual(result.Content.StateNumber, item.StateNumber);
            Assert.AreEqual(result.Content.VIN, item.VIN);
            Assert.AreEqual(result.Content.IdUsers, item.IdUsers);
        }

        [TestMethod]
        public void PostProduct_ShouldFail_WhenModel()
        {
            var controller = new MachinesController(new TestMachineContextGood());
            var item = new Machine
            {
                Type = "DemoType",
                Mark = "DemoMark",
                Model = "DemoModel",
                Year = 2000,
                StateNumber = "DemoNum",
                VIN = "DemoVIN",
                IdUsers = 1
            };

            var result = controller.PostMachine(item).Result as CreatedAtRouteNegotiatedContentResult<Machine>;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PutProduct_ShouldReturnStatusCode()
        {
            var controller = new MachinesController(new TestMachineContextGood());

            var item = GetDemoProduct();

            var result = controller.PutMachine(item.Id, item).Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public void PutProduct_ErrorContext()
        {
            var controller = new MachinesController(new TestMachineContextBad());

            Machine item = new Machine
            {
                Type = "DemoType",
                Mark = null,
                Model = null,
                Year = 1990,
                StateNumber = "DemoNum",
                VIN = null,
                IdUsers = 8
            };

            var result = controller.PutMachine(item.Id, item).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenDifferentID()
        {
            var controller = new MachinesController(new TestMachineContextGood());

            var badresult = controller.PutMachine(999, GetDemoProduct()).Result;
            Assert.IsInstanceOfType(badresult, typeof(BadRequestResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel()
        {
            var context = new TestMachineContextGood();
            context.Machines.Add(GetDemoProduct());

            var controller = new MachinesController(context);

            Machine item = new Machine
            {
                Type = "DemoType",
                Mark = null,
                Model = null,
                Year = 1990,
                StateNumber = "DemoNum",
                VIN = null,
                IdUsers = 8
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutMachine(item.Id, item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel2()
        {
            var context = new TestMachineContextBad();
            context.Machines.Add(GetDemoProduct());

            var controller = new MachinesController(context);

            Machine item = new Machine
            {
                Id = 1,
                Type = "DemoType",
                Mark = "DemoMark",
                Model = "DemoModel",
                Year = 2000,
                StateNumber = "DemoNum",
                VIN = "DemoVIN",
                IdUsers = 1
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutMachine(1, item).Result as StatusCodeResult;

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [TestMethod]
        public void GetProduct_ShouldReturnProductWithSameID()
        {
            var context = new TestMachineContextGood();
            context.Machines.Add(GetDemoProduct());

            var controller = new MachinesController(context);
            var result = controller.GetMachine(1).Result as OkNegotiatedContentResult<Machine>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Content.Id);
        }

        [TestMethod]
        public void GetProduct_ShouldFail_WhenNoID()
        {
            var context = new TestMachineContextGood();
            context.Machines.Add(GetDemoProduct());

            var controller = new MachinesController(context);
            var result = controller.GetMachine(2).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetProducts_ShouldReturnAllProducts()
        {
            var context = new TestMachineContextGood();
            context.Machines.Add(new Machine { Id = 1, Type = "DemoType1", Mark = "DemoMark1", Model = "DemoModel1", Year = 2001, StateNumber = "DemoNum1", VIN = "DemoVIN1", IdUsers = 1 });
            context.Machines.Add(new Machine { Id = 2, Type = "DemoType2", Mark = "DemoMark2", Model = "DemoModel2", Year = 2002, StateNumber = "DemoNum2", VIN = "DemoVIN2", IdUsers = 2 });
            context.Machines.Add(new Machine { Id = 3, Type = "DemoType3", Mark = "DemoMark3", Model = "DemoModel3", Year = 2003, StateNumber = "DemoNum3", VIN = "DemoVIN3", IdUsers = 3 });
            
            var controller = new MachinesController(context);
            var result = controller.GetMachines() as TestDBSetMachines;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Local.Count);
        }

        [TestMethod]
        public void DeleteProduct_ShouldReturnOK()
        {
            var context = new TestMachineContextGood();
            var item = GetDemoProduct();
            context.Machines.Add(item);

            var controller = new MachinesController(context);
            var result = controller.DeleteMachine(1).Result as OkNegotiatedContentResult<Machine>;

            Assert.IsNotNull(result);
            Assert.AreEqual(item.Id, result.Content.Id);
        }

        [TestMethod]
        public void DeleteProduct_ShouldNotFound()
        {
            var context = new TestMachineContextGood();
            var item = GetDemoProduct();
            context.Machines.Add(item);

            var controller = new MachinesController(context);
            var result = controller.DeleteMachine(555).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void test_validation()
        {
            var context = new TestMachineContextGood();
            context.Machines.Add(GetDemoProduct());
            Machine item = new Machine
            {
                Id = 1,
                Type = "DemoType",
                Mark = string.Empty,
                Model = "DemoModel",
                Year = 2000,
                StateNumber = "DemoNum",
                VIN = "DemoVIN",
                IdUsers = 1
            };
            var controller = new MachinesController();
            controller.Configuration = new HttpConfiguration();

            controller.Validate(item);
            var result = controller.PostMachine(item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            Assert.AreEqual(1, context.Machines.Count());
        }

        Machine GetDemoProduct()
        {
            return new Machine() { Id = 1, Type = "DemoType", Mark = "DemoMark", Model = "DemoModel", Year = 2000, StateNumber = "DemoNum", VIN = "DemoVIN", IdUsers = 1 };
        }
    }
}
