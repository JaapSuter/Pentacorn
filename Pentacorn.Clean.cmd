@echo off

attrib -R -A -S -H /S /D

IF EXIST bin RMDIR /S /Q bin
IF EXIST obj RMDIR /S /Q obj

FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO IF EXIST "%%G" RMDIR /S /Q "%%G"
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO IF EXIST "%%G" RMDIR /S /Q "%%G"

del *.suo /s/q
del *.cachefile /s/q
