﻿using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using Kontur.ImageTransformer.Controllers;

namespace Kontur.ImageTransformer.Selectors
{
    /// <inheritdoc />
    /// <summary>
    /// Overload for response 400 on bad filter name 
    /// </summary>
    public class Http404ActionSelector : ApiControllerActionSelector
    {
        public override HttpActionDescriptor SelectAction(HttpControllerContext context)
        {
            HttpActionDescriptor decriptor;
            try
            {
                decriptor = base.SelectAction(context);
            }
            catch (HttpResponseException ex)
            {
                var code = ex.Response.StatusCode;
                if (code != HttpStatusCode.NotFound && code != HttpStatusCode.MethodNotAllowed)
                {
                    throw;
                }

                var routeData = context.RouteData;
                routeData.Values["action"] = "Handle404";
                var httpController = new BadRequestController();
                context.Controller = httpController;
                context.ControllerDescriptor =
                    new HttpControllerDescriptor(context.Configuration, "BadRequest", httpController.GetType());
                decriptor = base.SelectAction(context);
            }

            return decriptor;
        }
    }
}