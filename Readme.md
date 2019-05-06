<h1>An XSLT based Logger for .Net</h1>

<a href="http://www.youtube.com/watch?feature=player_embedded&v=ftJXZykrLvw
" target="_blank"><img src="https://farm6.staticflickr.com/5803/22400215051_f5d3801213_o.png" 
alt="Visual Logger Video" width="480" height="360" border="1" /></a>

A simple logger based on xml/xslt that can produce color coded with rich output
to help find problems in the log files easily.

Features:

* The logfiles are xml with embedded stylesheet, so that they can be opened 
directly in the browser

* User can create their own stylesheet to change the rendering of the log

* Logging to the database and Event Logs (TBD)

* Split log files based on time and size

* Auto purge to conserve space

There are many good log framework (like nlog, elmah etc.) that are more generic 
and expressive. This program is a simple framework that allows viewing
and manipulating log files in a browser. 

How to use:

* Copy the logger.xml file to the root of your project and mark the file as 
Copy to Output Directory in the Properties window

* When file program runs, it will automatically create a log folder in the 
project output.

* Double Click on the ViewLog.cmd file to see the logs in a browser window

```
		static Log log = null;
        private static LogConfig GetLogConfig()
        {
            string path = LogConstants.GetSetupDir() + @"\logger.xml";
            return LogConstants.DeserializeLogConfig(path);
        }

        public static void Main()
        {
            log = new Log(GetLogConfig())
            {
                DefaultCategory = "category", // Name should match the category in xml fil
                DefaultUser = "email1@email.com"
            };
		}
```

Note:

* File Log outputs html with xsl stylesheet. Because of security some browsers may not
be able to render the file properly. An open source mini webserver [devd](https://github.com/cortesi/devd) is included. Double
click on ViewLog to see the logs on the browser. If it does not show up on the browser try
http://localhost:8000 to see your logs...
