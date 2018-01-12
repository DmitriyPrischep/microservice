using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fines.Models;
using Fines.Controllers;
using System.Net;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web.Http.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestFine
    {
        [TestMethod]
        public void PostProduct_ShouldReturnSameProduct()
        {
            var controller = new FinesController(new TestFinesContextGood());

            var item = GetDemoProduct();

            var result =
                controller.PostFine(item).Result as CreatedAtRouteNegotiatedContentResult<Fine>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RouteName, "DefaultApi");
            Assert.AreEqual(result.RouteValues["id"], result.Content.Id);
            Assert.AreEqual(result.Content.NameFine, item.NameFine);
            Assert.AreEqual(result.Content.AmountFine, item.AmountFine);
            Assert.AreEqual(result.Content.Id, item.Id);
        }

        [TestMethod]
        public void PostProduct_ShouldFail_WhenModel()
        {
            var controller = new FinesController(new TestFinesContextGood());
            var item = new Fine { Id = 3,
                NameFine = "Demo",
                AmountFine = 100
            };

            var result = controller.PostFine(item).Result as CreatedAtRouteNegotiatedContentResult<Fine>;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PutProduct_ShouldReturnStatusCode()
        {
            var controller = new FinesController(new TestFinesContextGood());

            var item = GetDemoProduct();

            var result = controller.PutFine(item.Id, item).Result as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(StatusCodeResult));
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public void PutProduct_ErrorContext()
        {
            var controller = new FinesController(new TestFinesContextBad());

            Fine item = new Fine { Id = 3, NameFine = string.Empty, AmountFine = 100 };

            var result = controller.PutFine(item.Id, item).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenDifferentID()
        {
            var controller = new FinesController(new TestFinesContextGood());

            var badresult = controller.PutFine(999, GetDemoProduct()).Result;
            Assert.IsInstanceOfType(badresult, typeof(BadRequestResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel()
        {
            var context = new TestFinesContextGood();
            context.Fines.Add(GetDemoProduct());

            var controller = new FinesController(context);

            Fine item = new Fine { Id = 3, NameFine = string.Empty, AmountFine = 100 };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutFine(item.Id, item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public void PutProduct_ShouldFail_WhenModel2()
        {
            var context = new TestFinesContextBad();
            context.Fines.Add(GetDemoProduct());

            var controller = new FinesController(context);

            Fine item = new Fine
            {
                Id = 3,
                NameFine = "Demo",
                AmountFine = 100
            };

            controller.Configuration = new HttpConfiguration();
            controller.Validate(item);
            var result = controller.PutFine(3, item).Result as StatusCodeResult;

            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [TestMethod]
        public void GetProduct_ShouldReturnProductWithSameID()
        {
            var context = new TestFinesContextGood();
            context.Fines.Add(GetDemoProduct());

            var controller = new FinesController(context);
            var result = controller.GetFine(3).Result as OkNegotiatedContentResult<Fine>;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Content.Id);
        }

        [TestMethod]
        public void GetProduct_ShouldFail_WhenNoID()
        {
            var context = new TestFinesContextGood();
            context.Fines.Add(GetDemoProduct());

            var controller = new FinesController(context);
            var result = controller.GetFine(2).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetProducts_ShouldReturnAllProducts()
        {
            var context = new TestFinesContextGood();
            context.Fines.Add(new Fine { Id = 1, NameFine = "Demo1", AmountFine = 100 });
            context.Fines.Add(new Fine { Id = 2, NameFine = "Demo2", AmountFine = 200 });
            context.Fines.Add(new Fine { Id = 3, NameFine = "Demo3", AmountFine = 300 });

            var controller = new FinesController(context);
            var result = controller.GetFines() as TestDBSetFines;

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Local.Count);
        }

        [TestMethod]
        public void DeleteProduct_ShouldReturnOK()
        {
            var context = new TestFinesContextGood();
            var item = GetDemoProduct();
            context.Fines.Add(item);

            var controller = new FinesController(context);
            var result = controller.DeleteFine(3).Result as OkNegotiatedContentResult<Fine>;

            Assert.IsNotNull(result);
            Assert.AreEqual(item.Id, result.Content.Id);
        }

        [TestMethod]
        public void DeleteProduct_ShouldNotFound()
        {
            var context = new TestFinesContextGood();
            var item = GetDemoProduct();
            context.Fines.Add(item);

            var controller = new FinesController(context);
            var result = controller.DeleteFine(555).Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.GetType(), typeof(NotFoundResult));
        }

        [TestMethod]
        public void test_validation()
        {
            var context = new TestFinesContextGood();
            context.Fines.Add(GetDemoProduct());
            Fine item = new Fine {
                Id = 3,
                NameFine = string.Empty,
                AmountFine = 100
            };
            var controller = new FinesController();
            controller.Configuration = new HttpConfiguration();

            controller.Validate(item);
            var result = controller.PostFine(item).Result;

            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            Assert.AreEqual(1, context.Fines.Count());
        }

        Fine GetDemoProduct()
        {
            return new Fine() { Id = 3, NameFine = "Demo", AmountFine = 100 };
        }
    }
}
