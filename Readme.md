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

* Copy the logger.xml file to your folder and make changes as needed

* After running, open the index.xml file directly on a latest browser 
(Chrome, and Firefox works, IE Edge currently does not work because of
security)

* Or create a web application pointing to the log folder and run index.xml 
(IE browser works here)

Note:

* File Log outputs html with xsl stylesheet. Because of security some browsers may not
be able to render the file properly. An open source mini webserver devx is included. Double
click on ViewLog to see the logs on the browser. If it does not show up on the browser try
http://localhost:8000 to see your logs...
