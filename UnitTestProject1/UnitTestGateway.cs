using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceFines.Models;
using ServiceFines.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http.Results;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTestGateway
    {

        [TestMethod]
        public void TestMethod1()
        {
            var controller = new GatewayController();
            var result = controller.Get("users").Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod2()
        {
            var controller = new GatewayController();
            var result = controller.Get("machines").Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod3()
        {
            var controller = new GatewayController();
            var result = controller.Get("fines").Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var controller = new GatewayController();
            var result = controller.GetAll("_").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var controller = new GatewayController();
            var result = controller.GetAll("Rowan Miller").Result;
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void TestMethod5_1()
        {
            var controller = new GatewayController();
            var result = controller.Get("Ivanov Artem").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod6()
        {
            var controller = new GatewayController();
            var result = controller.GetAllFines("_").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod7()
        {
            var controller = new GatewayController();
            var result = controller.GetAllFines("Неправильная парковка").Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod8()
        {
            var controller = new GatewayController();
            var result = controller.GetAllUsers(1, 3);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod9()
        {
            var controller = new GatewayController();
            var result = controller.Get("users", 1).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod10()
        {
            var controller = new GatewayController();
            var result = controller.Get("machines", 1).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod11()
        {
            var controller = new GatewayController();
            var result = controller.Get("fines", 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod12()
        {
            var controller = new GatewayController();
            var result = controller.Get("users", 0).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod13()
        {
            var controller = new GatewayController();
            var result = controller.Get("machines", 0).Result;

            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void TestMethod14()
        {
            var controller = new GatewayController();
            var result = controller.Get("fines", 0).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod15()
        {
            var controller = new GatewayController();
            var result = controller.Get(1, 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod16()
        {
            var controller = new GatewayController();
            var result = controller.GetAllFines("Неправильная парковка", 1, 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod17()
        {
            var controller = new GatewayController();
            var result = controller.GetAllFines("Проезд на красный свет", 1, 1).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod18()
        {
            var controller = new GatewayController();
            var item = new UserModel
            {
                FIO = "Loza Ivan",
                Adress = "Moscow, Liteynaya d13 kv55",
                Phone = "89151246578",
                IdFines = 1
            };
            var result = controller.Get(item).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod19()
        {
            var controller = new GatewayController();
            var item = new MachineModel
            {
                Id = 1,
                Type = "automobile",
                Mark = "Ford",
                Model = "Mondeo",
                Year = 2011,
                StateNumber = "a001yy77",
                VIN = "",
                IdUsers = 1
            };
            var result = controller.Get(item).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod20()
        {
            var controller = new GatewayController();
            var item = new DetailUserModel
            {
                FIO = "Loza Ivan",
                Adress = "Moscow, Liteynaya d13 kv55",
                Phone = "89151246578",
                Mark = "Acura",
                Model = "MDX",
                StateNumber = "t543mo99",
                NameFine = "Пересечение стоп линии",
                AmountFine = 1000
            };
            var result = controller.Post(item).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod21()
        {
            var controller = new GatewayController();
            var item = new UserModel
            {
                FIO = "Loza Ivan",
                Adress = "Moscow, Liteynaya d13 kv55",
                Phone = "89151246578",
                IdFines = 1
            };

            var result1 = controller.Get(item).Result as OkNegotiatedContentResult<UserModel>;
            result1.Content.FIO = "Loza Victor";
            var result = controller.Put(result1.Content.Id, result1.Content).Result;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestMethod22()
        {
            var controller = new GatewayController();
            var result = controller.Delete("Loza Victor").Result;

            Assert.IsNotNull(result);
        }


    }
}
