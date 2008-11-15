@echo off
rem script ssds console 

rem NOTE: 
rem set ssdsUser and ssdsPassword in the config file
rem modify this script to use your own authority

set authority=your-authority

sds /%authority%/ get
sds /%authority%/fish post
sds /%authority%/fish get
sds /%authority%/fish/ post fish-001.xml
sds /%authority%/fish/ post fish-002.xml
sds /%authority%/fish/ get
sds /%authority%/fish/fish-001 get
sds /%authority%/fish/fish-002 delete
sds /%authority%/fish/ get

set authority=

rem ##eof##