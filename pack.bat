@echo off

setlocal

set name=NginxHelper
set releasedir=.\%name%\%name%\bin\Release
set executable=%releasedir%\%name%.exe
if not exist %executable% (
    nircmd.exe infobox "Please build the release version first!" ""
    exit
)

copy /b %executable% ng.exe /y
7z.exe a "%name%.zip" ng.exe -sdel
7z.exe a "%name%.zip" sample.conf sample.ini LICENSE readme.md

endlocal
