<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" id="stylesheet"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output
    method="html"
				encoding="utf-8"
				indent="yes" />

  <xsl:strip-space elements="*" />

  <xsl:template match="/logs">
    <html>
      <head>
        <style type="text/css">
          .show-more {
          display: none;
          }
          .click-show-more {
          text-decoration: underline;
          color: blue;
          cursor: pointer
          }
          table {
          border-collapse: collapse;
          width: 99%;
          margin-top: 5px;
          }

          table, th, td {
          border: 1px solid grey;
          }

          th, td {
          vertical-align: top;
          padding: 5px;
          text-align: left;
          }

          th{
          background-color: black;
          color: white;
          }

          ul{
          list-style-type: circle;
          }

          li:nth-child(odd){
          background: rgba(0,0,0,.1);
          }
        </style>
        <script src="http://code.jquery.com/jquery-latest.min.js"
        type="text/javascript"></script>
        <script type='text/javascript'>
          //<![CDATA[
              $(window).on('load', function() {
                var $rows = $('#table tr');
                $('#search').keyup(function() {
                    var val = $.trim($(this).val()).replace(/ +/g, ' ').toLowerCase();
    
                    $rows.show().filter(function() {
                        var text = $(this).text().replace(/\s+/g, ' ').toLowerCase();
                        return !~text.indexOf(val);
                    }).hide();
                });
              });
              
              $(document).ready(function() {
                  $(".click-show-more").click(function () {
                    var $closestContent = $(this).closest('.article').find('.content');
    
                    if($closestContent.hasClass("show-more")) {
                        $(this).text("Hide Stack Trace");
                    } else {
                        $(this).text("Show Stack Trace");
                    }
                    $closestContent.toggleClass('show-more');
                  });
              });
              //]]>
        </script>
      </head>
      <body>
        <input type="text" id="search" placeholder="Type to filter" />
        <table id="table">
          <tr>
            <th>
              Date
            </th>
            <th>
              Id
            </th>
            <th>
              State
            </th>
            <th>
              Category
            </th>
            <th>
              User
            </th>
            <th>
              Title
            </th>
            <th>
              Description
            </th>
          </tr>
          <xsl:apply-templates select="log" />
        </table>
      </body>
    </html>

  </xsl:template>

  <xsl:template match="log">
    <xsl:variable name="backColor">
      <xsl:choose>
        <xsl:when test="s = 'Debug'">
          <xsl:text>LightBlue</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'Event'">
          <xsl:text>Yellow</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'Warning'">
          <xsl:text>PaleGoldenRod</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'Pass'">
          <xsl:text>LightGreen</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'Title'">
          <xsl:text>Khaki</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'SubTitle'">
          <xsl:text>LightGrey</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'Error'">
          <xsl:text>LightPink</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'FatalError'">
          <xsl:text>Violet</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'Function'">
          <xsl:text>#F0F0F0</xsl:text>
        </xsl:when>
        <xsl:when test="s = 'Method'">
          <xsl:text>#F0F0F0</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>inherit</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <tr style="background-color:{$backColor}">
      <td style="white-space: nowrap">
        <xsl:value-of select="dt"/>
        <br/>
        <xsl:value-of select="tm"/>
      </td>
      <td>
        <xsl:value-of select="id"/>:<xsl:value-of select="th"/>
      </td>
      <td>
        <xsl:value-of select="s"/>
      </td>
      <td>
        <xsl:value-of select="c"/>
      </td>
      <td>
        <xsl:value-of select="u"/>
      </td>
      <td>
        <xsl:value-of select="t"/>
      </td>
      <td>
        <xsl:value-of select="d" disable-output-escaping="yes"/>
        <xsl:if test="st != ''">
          <br/>
          <div class="article">
            <div class="content show-more">
              <ul>
                <xsl:for-each select="st/stl">
                  <li>
                    <xsl:value-of select="." disable-output-escaping="yes" />
                  </li>
                </xsl:for-each>
              </ul>
            </div>
            <div class="click-show-more">Show Stack Trace</div>
          </div>
        </xsl:if>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
