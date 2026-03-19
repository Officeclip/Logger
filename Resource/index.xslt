<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    id="stylesheet"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="html" encoding="utf-8" indent="yes" />
  <xsl:strip-space elements="*" />

  <xsl:template match="/files">
    <html>
      <head>
        <title>Logs</title>
        <style>
          html, body {
            margin: 0;
            padding: 0;
            height: 100%;
            overflow: hidden;
            font-family: Arial, sans-serif;
            font-size: 13px;
          }

          .wrap {
            display: flex;
            height: 100vh;
          }

          .left {
            width: 300px;
            overflow-y: auto;
            border-right: 1px solid #ccc;
            padding: 10px;
            box-sizing: border-box;
            background: #fafafa;
          }

.right {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-sizing: border-box;
  background: #fff;
}		

.toolbar {
  flex: 0 0 auto;
  background: #fff;
  z-index: 40;
  border-bottom: 1px solid #ddd;
  padding: 10px;
  box-sizing: border-box;
}

.toolbarRow {
  display: flex;
  gap: 8px;
  align-items: center;
  margin: 0;
  padding: 0;
}

.tableWrap {
  flex: 1 1 auto;
  overflow: auto;
  padding: 0 10px 10px 10px;
  box-sizing: border-box;
}

          .toolbar input[type="text"] {
            width: 320px;
            max-width: 100%;
            padding: 6px 8px;
            border: 1px solid #bbb;
            border-radius: 4px;
            box-sizing: border-box;
          }

          .statusText {
            color: #666;
            font-size: 12px;
          }

          .logLink {
            display: block;
            padding: 6px 8px;
            margin-bottom: 4px;
            text-decoration: none;
            color: #222;
            border-radius: 4px;
            word-break: break-word;
          }

          .logLink:hover {
            background: #eaeaea;
          }

          .logLink.active {
            background: #dfefff;
            font-weight: bold;
          }

table {
  border-collapse: separate;
  border-spacing: 0;
  width: 100%;
  table-layout: fixed;
  margin-top: 0;
}

table, th, td {
  border: 1px solid #bdbdbd;
}

th, td {
  vertical-align: top;
  padding: 6px;
  text-align: left;
  word-break: break-word;
}

th {
  background: #222;
  color: #fff;
  position: sticky;
  top: 0;
  z-index: 20;
  line-height: 1.4;
}

          .colDate { width: 120px; }
          .colId { width: 70px; }
          .colState { width: 90px; }
          .colCategory { width: 140px; }
          .colUser { width: 120px; }
          .colTitle { width: 220px; }

          .stackTrace {
            display: none;
            margin-top: 8px;
            padding: 8px;
            background: #f7f7f7;
            border: 1px solid #ddd;
          }

          .stackTrace.show {
            display: block;
          }

          .stackTrace ul {
            margin: 0;
            padding-left: 20px;
          }

          .stackTrace li {
            font-family: Consolas, monospace;
            font-size: 12px;
            line-height: 1.45;
            margin-bottom: 4px;
          }

          .toggleLink {
            margin-top: 8px;
            color: #0a58ca;
            text-decoration: underline;
            cursor: pointer;
            display: inline-block;
          }

          .emptyMsg {
            color: #666;
            padding: 16px 0;
          }

          .loadingMsg {
            color: #666;
            padding: 16px 0;
          }
        </style>

        <script type="text/javascript"><![CDATA[
          var currentLogUrl = '';
          var refreshMs = 3000;
          var loading = false;
          var lastLogXmlText = '';
          var selectedLinkEl = null;

          function htmlEncode(str) {
            if (str === null || str === undefined) return '';
            return String(str)
              .replace(/&/g, '&amp;')
              .replace(/</g, '&lt;')
              .replace(/>/g, '&gt;')
              .replace(/"/g, '&quot;')
              .replace(/'/g, '&#39;');
          }

          function getText(parent, tagName) {
            var nodes = parent.getElementsByTagName(tagName);
            if (!nodes || !nodes.length || !nodes[0].firstChild) return '';
            return nodes[0].textContent || nodes[0].firstChild.nodeValue || '';
          }

          function getBackColor(state) {
            switch (state) {
              case 'Debug': return 'LightBlue';
              case 'Event': return 'Yellow';
              case 'Warning': return 'PaleGoldenRod';
              case 'Pass': return 'LightGreen';
              case 'Title': return 'Khaki';
              case 'SubTitle': return 'LightGrey';
              case 'Error': return 'LightPink';
              case 'FatalError': return 'Violet';
              case 'Function': return '#F0F0F0';
              case 'Method': return '#F0F0F0';
              default: return 'inherit';
            }
          }

          function setActive(linkEl) {
            var links = document.getElementsByClassName('logLink');
            for (var i = 0; i < links.length; i++) {
              links[i].className = 'logLink';
            }
            if (linkEl) {
              linkEl.className = 'logLink active';
            }
            selectedLinkEl = linkEl;
          }

          function renderLogs(xmlText, preserveScroll) {
            var parser = new DOMParser();
            var xmlDoc = parser.parseFromString(xmlText, 'text/xml');

            if (xmlDoc.getElementsByTagName('parsererror').length > 0) {
              document.getElementById('logArea').innerHTML =
                '<div class="emptyMsg">Could not parse log XML.</div>';
              return;
            }

            var logs = xmlDoc.getElementsByTagName('log');
            var rightPane = document.getElementById('rightPane');
            var oldScrollTop = rightPane.scrollTop;
            var nearBottom = (rightPane.scrollHeight - (rightPane.scrollTop + rightPane.clientHeight)) < 80;

            var html = '';
            html += '<table id="logTable">';
            html += '<thead><tr>';
            html += '<th class="colDate">Date</th>';
            html += '<th class="colId">Id</th>';
            html += '<th class="colState">State</th>';
            html += '<th class="colCategory">Category</th>';
            html += '<th class="colUser">User</th>';
            html += '<th class="colTitle">Title</th>';
            html += '<th>Description</th>';
            html += '</tr></thead>';
            html += '<tbody>';

            for (var i = 0; i < logs.length; i++) {
              var log = logs[i];

              var dt = getText(log, 'dt');
              var tm = getText(log, 'tm');
              var s = getText(log, 's');
              var c = getText(log, 'c');
              var u = getText(log, 'u');
              var t = getText(log, 't');
              var d = getText(log, 'd');
              var id = getText(log, 'id');
              var th = getText(log, 'th');

              var stackNodes = log.getElementsByTagName('stl');
              var stackId = 'stack_' + i;
              var backColor = getBackColor(s);

              html += '<tr style="background-color:' + backColor + '">';
              html += '<td style="white-space:nowrap;">' + htmlEncode(dt) + '<br />' + htmlEncode(tm) + '</td>';
              html += '<td>' + htmlEncode(id) + ':' + htmlEncode(th) + '</td>';
              html += '<td>' + htmlEncode(s) + '</td>';
              html += '<td>' + htmlEncode(c) + '</td>';
              html += '<td>' + htmlEncode(u) + '</td>';
              html += '<td>' + htmlEncode(t) + '</td>';
              html += '<td>';

              html += htmlEncode(d).replace(/\r?\n/g, '<br />');

              if (stackNodes.length > 0) {
                html += '<div class="toggleLink" onclick="toggleStackTrace(\'' + stackId + '\', this)">Show Stack Trace</div>';
                html += '<div id="' + stackId + '" class="stackTrace"><ul>';

                for (var j = 0; j < stackNodes.length; j++) {
                  var stText = stackNodes[j].textContent || stackNodes[j].text || '';
                  html += '<li>' + htmlEncode(stText) + '</li>';
                }

                html += '</ul></div>';
              }

              html += '</td>';
              html += '</tr>';
            }

            html += '</tbody></table>';

            document.getElementById('logArea').innerHTML = html;
            applyFilter();

            if (preserveScroll) {
              if (nearBottom) {
                rightPane.scrollTop = rightPane.scrollHeight;
              } else {
                rightPane.scrollTop = oldScrollTop;
              }
            }
          }

          function toggleStackTrace(stackId, linkEl) {
            var el = document.getElementById(stackId);
            if (!el) return false;

            if (el.className.indexOf('show') >= 0) {
              el.className = 'stackTrace';
              linkEl.innerHTML = 'Show Stack Trace';
            } else {
              el.className = 'stackTrace show';
              linkEl.innerHTML = 'Hide Stack Trace';
            }
            return false;
          }

          function updateStatus(text) {
            var el = document.getElementById('statusText');
            if (el) {
              el.innerHTML = text || '';
            }
          }

          function loadLog(url, linkEl, isAuto) {
            if (loading) return false;
            loading = true;

            currentLogUrl = url;

            if (!isAuto) {
              setActive(linkEl);
              lastLogXmlText = '';
              document.getElementById('logArea').innerHTML = '<div class="loadingMsg">Loading...</div>';
            }

            var xhr = new XMLHttpRequest();
            var sep = url.indexOf('?') >= 0 ? '&' : '?';
            xhr.open('GET', url + sep + '_ts=' + new Date().getTime(), true);

            xhr.onreadystatechange = function () {
              if (xhr.readyState === 4) {
                loading = false;

                if (xhr.status >= 200 && xhr.status < 300) {
                  var xmlText = xhr.responseText || '';

                  if (xmlText === lastLogXmlText) {
                    updateStatus('No change');
                    return;
                  }

                  lastLogXmlText = xmlText;
                  renderLogs(xmlText, isAuto ? true : false);
                  updateStatus('Updated ' + new Date().toLocaleTimeString());
                } else {
                  if (!isAuto) {
                    document.getElementById('logArea').innerHTML =
                      '<div class="emptyMsg">Could not load log file.</div>';
                  }
                  updateStatus('Load failed');
                }
              }
            };

            xhr.send(null);
            return false;
          }

          function refreshLog() {
            if (!currentLogUrl) return;
            loadLog(currentLogUrl, selectedLinkEl, true);
          }

          function applyFilter() {
            var input = document.getElementById('search');
            var table = document.getElementById('logTable');
            if (!input || !table) return;

            var val = input.value.toLowerCase().replace(/\s+/g, ' ').trim();
            var rows = table.getElementsByTagName('tr');

            for (var i = 1; i < rows.length; i++) {
              var text = rows[i].textContent || rows[i].innerText || '';
              text = text.toLowerCase().replace(/\s+/g, ' ').trim();

              if (val === '' || text.indexOf(val) >= 0) {
                rows[i].style.display = '';
              } else {
                rows[i].style.display = 'none';
              }
            }
          }

          window.onload = function () {
            var search = document.getElementById('search');
            if (search) {
              search.onkeyup = applyFilter;
            }

            var first = document.getElementById('firstLogLink');
            if (first) {
              first.onclick();
            }

            window.setInterval(refreshLog, refreshMs);
          };
        ]]></script>
      </head>

      <body>
        <div class="wrap">
          <div class="left">
            <xsl:apply-templates select="file" />
          </div>

			<div class="right">
			  <div class="toolbar">
				<div class="toolbarRow">
				  <input type="text" id="search" placeholder="Type to filter" />
				  <span class="statusText" id="statusText"></span>
				</div>
			  </div>
			  <div class="tableWrap" id="rightPane">
				<div id="logArea">
				  <div class="emptyMsg">Select a log.</div>
				</div>
			  </div>
			</div>

        </div>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="file">
    <a href="{name}" class="logLink">
      <xsl:if test="position() = 1">
        <xsl:attribute name="id">firstLogLink</xsl:attribute>
      </xsl:if>
      <xsl:attribute name="onclick">return loadLog('<xsl:value-of select="name" />', this, false);</xsl:attribute>
      <xsl:value-of select="display" />
    </a>
  </xsl:template>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()" />
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>