<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" id="stylesheet"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output
    method="html"
				encoding="utf-8"
				indent="yes" />

  <xsl:strip-space elements="*" />

  <xsl:template match="/files">
    <html>
      <body>
        <table border="1">
          <tr>
            <th>
              Name
            </th>
            <th>
              Lines
            </th>
            <th>
              Fatal
            </th>
            <th>
              Error
            </th>
            <th>
              Warning
            </th>
          </tr>
          <xsl:apply-templates select="file" />
        </table>
      </body>
    </html>

  </xsl:template>

  <xsl:template match="file">
    <tr>
      <td style="white-space: nowrap">
        <a href="{name}">
          <xsl:value-of select="display"/>
        </a>
      </td>
      <td>
        <xsl:value-of select="lines"/>
      </td>
      <td>
        <xsl:value-of select="fatal"/>
      </td>
      <td>
        <xsl:value-of select="error"/>
      </td>
      <td>
        <xsl:value-of select="warning"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
