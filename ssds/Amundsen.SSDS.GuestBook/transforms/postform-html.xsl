<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:s="http://schemas.microsoft.com/sitka/2008/03/">
	<xsl:output method="html" encoding="utf-8" omit-xml-declaration ="yes"/>

	<xsl:variable name="domain-prefix" select="'http://www.amundsen.com/guestbook/messages/'" />
	<xsl:param name="build-date" />
	<xsl:param name="title" select="'Recent SSDS Guestbook Messages'"/>
	<xsl:param name="max-rows" select="25"/>

	<!-- main portion template -->
	<html>
		<head>
			<title>SSDS Guestbook HTML</title>
		</head>
		<body>
			<h1>SSDS Guestbook HTML</h1>
			<form action="./" method="post">
				<textarea name="message=" rows="3" cols="40"></textarea>
			<br />
			<input type="submit" value="Post" />
			<input type="reset" value="Reset" />
			</form>
			<div id="messaages">
				<dl>
					<dt>
						<span class="nick">
							<xsl:value-of select="nickname" />
						</span>
						<span class="date">
							<xsl:value-of select="date-created" />
						</span>
					</dt>
					<dd>
						<xsl:value-of select="normalize-space(body)"/>
					</dd>
				</dl>
			</div>
		</body>
	</html>
	
</xsl:stylesheet>

