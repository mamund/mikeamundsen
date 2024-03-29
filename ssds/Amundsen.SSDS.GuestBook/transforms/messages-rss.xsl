<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:s="http://schemas.microsoft.com/sitka/2008/03/" exclude-result-prefixes="s">
	<xsl:output method="xml" encoding="utf-8" omit-xml-declaration ="yes" />

	<xsl:variable name="link-url" select="'http://amundsen.com/examples/ssds/guestbook-client/?filter='" />
	<xsl:variable name="id-url" select="'http://www.amundsen.com/guestbook/messages/'" />
	<xsl:param name="build-date" />
	<xsl:param name="title" select="'Recent SSDS Guestbook Messages'"/>
	<xsl:param name="max-rows" select="25"/>

	<!-- main portion template -->
	<xsl:template match="/">
		<rss version="2.0">
			<channel>
				<title><xsl:value-of select="$title"/></title>
				<link>http://www.amundsen.com/guestbook/feed/</link>
				<description>Recent messages posted to the SSDS Guestbook</description>
				<lastBuildDate>
					<xsl:value-of select="$build-date"/>
				</lastBuildDate>
				<language>en-us</language>
				<ttl>10</ttl>
			</channel>
			<xsl:apply-templates select="//message" />
		</rss>
	</xsl:template>

	<xsl:template match="message">
		<xsl:if test="position()&lt;=$max-rows">
			<item>
				<title>
					<xsl:value-of select="nickname" />
				</title>
				<link>
					<xsl:value-of select="$link-url" />
					<xsl:value-of select="nickname"/>&amp;id=<xsl:value-of select="s:Id"/>
				</link>
				<guid>
					<xsl:value-of select="$link-url" />
					<xsl:value-of select="nickname"/>&amp;id=<xsl:value-of select="s:Id"/>
				</guid>
				<pubDate>
					<xsl:value-of select="date-created" />
				</pubDate>
				<description>
					<xsl:value-of select="normalize-space(body)"/>
				</description>
			</item>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>

