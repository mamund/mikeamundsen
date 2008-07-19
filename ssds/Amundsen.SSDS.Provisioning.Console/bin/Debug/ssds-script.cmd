@echo off
rem script ssds console 

rem NOTE: 
rem set ssdsUser and ssdsPassword in the config file
rem modify this script to use an your authority 

set authority=mamund

ssds get    /%authority%
ssds post   /%authority%/fish
ssds get    /%authority%/fish
ssds post   /%authority%/fish/ fish-001.xml
ssds post   /%authority%/fish/ fish-002.xml
ssds get    /%authority%/fish/ 
ssds get    /%authority%/fish/fish-001 
ssds delete /%authority%/fish/fish-002
ssds get    /%authority%/fish/

set authority=
