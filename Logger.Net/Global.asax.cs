using System;
using System.Web;
using OfficeClip.OpenSource.Logger;
using StackExchange.Profiling;

namespace Logger.Net
{
    public class Global : System.Web.HttpApplication
    {
        public static Log Log;
        private bool isStarted;
        protected void Application_Start(object sender, EventArgs e)
        {
            this.isStarted = false;
            Log = new Log(Server.MapPath("logger.xml"));
            Log.WriteInfo("State", "Logger Started");
        }

        protected void Application_BeginRequest()
        {
            if (!isStarted)
            {
                isStarted = true;
                if (Request.IsLocal)
                {
                    MiniProfiler.Start();
                }
            }
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Stop();
        }

        protected void Application_End(
            object sender,
            EventArgs e)
        {
            Log.WriteInfo("State", "Logger Ended");
        }
    }
}