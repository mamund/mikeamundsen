@echo off
rem script ssds console 

rem NOTE: 
rem set ssdsUser and ssdsPassword in the config file
rem modify this script to use your own authority

set authority=your-authority

ssds /%authority%/ get
ssds /%authority%/fish post
ssds /%authority%/fish get
ssds /%authority%/fish/ post fish-001.xml
ssds /%authority%/fish/ post fish-002.xml
ssds /%authority%/fish/ get
ssds /%authority%/fish/fish-001 get
ssds /%authority%/fish/fish-002 delete
ssds /%authority%/fish/ get

set authority=

rem ##eof##