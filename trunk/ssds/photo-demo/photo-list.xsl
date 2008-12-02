<?xml version="1.0" encoding="utf-8"?>
<!-- 2008-11-10 (mca) : convert entity list into html page -->
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:s="http://schemas.microsoft.com/sitka/2008/03/" 
	xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
	xmlns:x="http://www.w3.org/2001/XMLSchema"
	xmlns="http://www.w3.org/1999/xhtml"
	extension-element-prefixes="s xsi x"
	exclude-result-prefixes="xmlns">
	
	<xsl:output method="xml" 
		omit-xml-declaration="no" 
		doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN" 
		doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd" 
		encoding="utf-8"
		media-type="text/html"
		standalone="yes"/>
	
	<xsl:template match="/">
		<html>
			<head>
				<title>SDS Photo Demo</title>
				<link href="./files/photos.css" type="text/css" rel="stylesheet" />
				<script type="text/javascript" src="./files/photos.js">/* photo.js */</script>
				<meta name="author" content="mike amundsen"/>
				<meta name="version" content="1.1 (mca) : 2008-11-15"/>
				<meta name="location" content="http://amundsen.com/examples/sds-photos/"/>
			</head>
			<body>
				<h1>SDS Photo Demo</h1>
				
				<address>
					Version 1.1 (mca) : 2008-11-15<br />
					<a href="http://amundsen.com/ssds/" title="SSDS Examples">SDS Examples</a>
				</address>

				<p>
					This demo stores @200 original images (plus another 400 thumbnail and preview versions) along with JS and CSS files
					directly in SDS storage (about 80MB in all). A small C# ASP.NET handler accepts anonymous requests, processes
					them and, when needed initiates authenticated requests to an SDS-Proxy that then talks directly to SDS
					servers to get the images, scripts, and stylesheets.
					The home page is HTML generated using XSLT against the Entity list returned from SDS.
					Follow the links at the bottom of the page to view the source code for this demo application. Check out the
					<a href="http://amundsen.com/ssds/" title="S[S]DS">SDS Examples</a> page for more details and a link
					to download this and other sample projects.
				</p>
				<p>
					NOTE: Minimal caching is used on the client in order to show the speed of SDS even when using intermediaries
					such as the SDS-Proxy. Images (some over 1MB) are currently cached for ten minutes to show relative speed during a single session.
					In a production application, additional caching would be used to reduce avg. page load to less than 1 sec.
				</p>
				<h4>Click on an image in the list to see the preview. Click on the preview image to see the full image.</h4>
				
				<div id="page">
					<div id="load-list">
						<img src="./files/bigrotation.gif"/>
						<p>
							Loading 200 images...
						</p>
					</div>
					<div id="detail">
						<a id="full-view" title="view full image" target="photo">
							<img id="preview" style="display:none;" />
						</a>
					</div>
					<div id="list">
						<ul id="photos" style="display:none;">
							<xsl:apply-templates select="//s:Entity" />
						</ul>
					</div>
					<div id="source" style="display:none;">
						<h4>Source Code</h4>
						<ul id="source-list">
							<li>
								<a href="./files/photo-demo-cs.txt" title="C# ASP.NET Handler" target="source">photo-demo.cs</a>
							</li>
							<li>
								<a href="./files/photo-list-xsl.txt" title="XSL transform" target="source">photo-list.xsl</a>
							</li>
							<li>
								<a href="./files/photos-script.txt" title="client-side script" target="source">photos.js</a>
							</li>
							<li>
								<a href="./files/photos-css.txt" title="stylesheet" target="source">photos.css</a>
							</li>
						</ul>
					</div>
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="s:Entity">
		<li>
			<a href="./preview/{s:Id}" title="#{position()} - {s:Id}">
				<img src="./thumbnail/{s:Id}" alt="{s:Id}" title="#{position()} - {s:Id} ({s:Content/@content-length} bytes)" />
			</a>
		</li>
	</xsl:template>
	
</xsl:stylesheet>
