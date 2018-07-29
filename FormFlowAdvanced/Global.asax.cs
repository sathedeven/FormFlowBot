using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using FormFowBasic;

namespace FormFlowAdvanced
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configuration.Filters.Add(new TravelExceptionFilterAttribute());
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
