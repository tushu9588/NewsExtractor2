﻿using NewsExtractor2;
using System.Web.Http;

namespace NewsExtractor2
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}