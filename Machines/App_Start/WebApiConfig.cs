﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Machines.Models;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace Machines
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            config.Filter().Expand().Select().OrderBy().MaxTop(null).Count();
            builder.EntitySet<Machine>("MachineInf");
            config.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: builder.GetEdmModel());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
