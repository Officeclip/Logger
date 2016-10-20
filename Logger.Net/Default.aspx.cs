using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using StackExchange.Profiling;

namespace Logger.Net
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MyLog myLog = new MyLog("category-1", "user1@email.com");
            var profiler = MiniProfiler.Current;
            using (profiler.Step("Start Logging"))
            {
                myLog.WriteError(
                                 "default.aspx",
                                 "Test Error");
                myLog.WriteWarning(
                                   "default.aspx",
                                   "Test Warning");
                myLog.WriteInfo(
                                "default.aspx",
                                "Test Info");
            }
            profiler.Step("Stop Logging");
            literal1.Text = profiler.Render().ToHtmlString();
        }
    }
}