using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Users.Controllers;
using Users.Models;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web.Http.Results;
using System.Net;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestUsers
    {
        [TestMethod]
        public void PostProduct_ShouldReturnSameProduct()
        {
            var controller = new UsersController(new TestUsersContextGood());

            var item = GetDemoProduct();

            var result =
                controller.PostUser(item).Result as CreatedAtRouteNegotiatedContentResult<User>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RouteName, "DefaultApi");
            Assert.AreEqual(result.RouteValues["id"], result.Content.Id);
            Assert.AreEqual(result.Content.FIO, item.FIO);
            Assert.AreEqual(result.Content.Adress, item.Adress);
            Assert.AreEqual(result.Content.Phone, item.Phone);
            Assert.AreEqual(result.Content.IdFines, item.IdFines);
            Assert.AreEqual(result.Content.Id, item.Id);
        }

        [TestMethod]
        public void PostProduct_ShouldFail_WhenModel()
        {
            var controller = new UsersController(new TestUsersContextGood());
            User item = new User
            {
                Id = 6,
                FIO = "Demo FIO",
                Adress = "Demo Adress",
                Phone = "Demo phone",
                IdFines = 1
            };

            var result = controller.PostUser(item).Result as CreatedAtRouteNegotiatedContentResult<User>;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PutProduct_ShouldReturnStatusCode()
        {
            var controller = new UsersController(new TestUsersContextGood());

            var item = GetDemoProduct();

            var result = controller.PutUser(item.Id, item).Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public void PutProduct_ErrorContext()
        {
            var controller = new UsersController(new TestUsersContextBad());
            User item = new User
            {
                Id = 3,
                FIO = "Demo FIO",
                Adress = "Demo Adress",
                Phone = "Demo phone",
                IdFines = 1
            };

            var result = controller.PutUser(item.Id, item).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenDifferentID()
        {
            var controller = new UsersController(new TestUsersContextGood());

            var badresult = controller.PutUser(999, GetDemoProduct()).Result;
            Assert.IsInstanceOfType(badresult, typeof(BadRequestResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel()
        {
            var context = new TestUsersContextGood();
            context.Users.Add(GetDemoProduct());

            var controller = new UsersController(context);

            User item = new User
            {
                Id = 1,
                FIO = string.Empty,
                Adress = "Demo Adress",
                Phone = "Demo phone",
                IdFines = 2
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutUser(item.Id, item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel2()
        {
            var context = new TestUsersContextBad();
            context.Users.Add(GetDemoProduct());

            var controller = new UsersController(context);

            User item = new User
            {
                Id = 1,
                FIO = "Demo FIO",
                Adress = "Demo Adress",
                Phone = "Demo phone",
                IdFines = 1
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutUser(1, item).Result as StatusCodeResult;

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [TestMethod]
        public void GetProduct_ShouldReturnProductWithSameID()
        {
            var context = new TestUsersContextGood();
            context.Users.Add(GetDemoProduct());

            var controller = new UsersController(context);
            var result = controller.GetUser(1).Result as OkNegotiatedContentResult<User>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Content.Id);
        }

        [TestMethod]
        public void GetProduct_ShouldFail_WhenNoID()
        {
            var context = new TestUsersContextGood();
            context.Users.Add(GetDemoProduct());

            var controller = new UsersController(context);
            var result = controller.GetUser(2).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetProducts_ShouldReturnAllProducts()
        {
            var context = new TestUsersContextGood();
            context.Users.Add(new User { Id = 1, FIO = "Demo name1", Adress = "Demo Adress", Phone = "Demo Phone", IdFines = 1 });
            context.Users.Add(new User { Id = 2, FIO = "Demo name2", Adress = "Demo Adress", Phone = "Demo Phone", IdFines = 2 });
            context.Users.Add(new User { Id = 3, FIO = "Demo name3", Adress = "Demo Adress", Phone = "Demo Phone", IdFines = 3 });
            var controller = new UsersController(context);
            var result = controller.GetUsers() as TestDBSetUsers;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Local.Count);
        }

        [TestMethod]
        public void DeleteProduct_ShouldReturnOK()
        {
            var context = new TestUsersContextGood();
            var item = GetDemoProduct();
            context.Users.Add(item);

            var controller = new UsersController(context);
            var result = controller.DeleteUser(1).Result as OkNegotiatedContentResult<User>;

            Assert.IsNotNull(result);
            Assert.AreEqual(item.Id, result.Content.Id);
        }

        [TestMethod]
        public void DeleteProduct_ShouldNotFound()
        {
            var context = new TestUsersContextGood();
            var item = GetDemoProduct();
            context.Users.Add(item);

            var controller = new UsersController(context);
            var result = controller.DeleteUser(555).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void test_validation()
        {
            var context = new TestUsersContextGood();
            context.Users.Add(GetDemoProduct());
            User item = new User
            {
                Id = 1,
                FIO = string.Empty,
                Adress = "Demo Adress",
                Phone = "Demo phone",
                IdFines = 1
            };

            var controller = new UsersController();
            controller.Configuration = new HttpConfiguration();

            controller.Validate(item);
            var result = controller.PostUser(item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            Assert.AreEqual(1, context.Users.Count());
        }


        User GetDemoProduct()
        {
            return new User() { Id = 1, FIO = "Demo FIO", Adress = "Demo Adress", Phone = "Demo phone", IdFines = 1 };
        }
    }
}
