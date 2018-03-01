@echo off

setlocal

set name=NginxHelper
set debugdir=.\%name%\%name%\bin\Debug
set inifile=%debugdir%\%name%.ini
if not exist %inifile%  (
    mklink /h %inifile% \bin\ng.ini
)

copy /b %debugdir%\%name%.exe \bin\ng.exe /y

endlocal
