@echo off
rem 
rem 2008-10-13 (mca) : make typed datasets from SSDS entity
rem

rem assumptions:
rem the current folder contains:
rem - valid XSD file for each entity 
rem - valid XSD file for sitka namespace
rem - d points to location of resulting CS dataset classes

rem set up
set d=..\entity-binding

rem kill old files
if exist %d%\task.cs del %d%\task.cs
if exist %d%\sitka.cs del %d%\sitka.cs

rem make new files
xsd task.xsd sitka.xsd /dataset /n:Amundsen.SSDS.Binding /o:%d%\

rem be happy
set d=
echo done!

rem
rem eof
rem