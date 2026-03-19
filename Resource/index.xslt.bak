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
        <div>
          <div style="float:left; border: 1px solid #CCC">
            <xsl:apply-templates select="file" />
          </div>
          <div style="overflow:hidden">
            <iframe name="FRAME4" style="width:100%; height:calc(100vh - 30px);" frameborder="0" ></iframe>
          </div>
        </div>
      </body>
    </html>

  </xsl:template>

  <xsl:template match="file">
    <a href="{name}" target="FRAME4">
      <xsl:value-of select="display"/>
    </a>
    <br/>
  </xsl:template>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
