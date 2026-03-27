@ECHO OFF

SETLOCAL EnableDelayedExpansion EnableExtensions

SET DIRCMD=D:\Users\ahulme\MICS#\_Testing\FtValidate\_BatchTestProgs

ECHO.
ECHO Creating Spoof Tables.
ECHO.
%MICSexeR%\Tools SpoofTablesForFtValidate

ECHO.
ECHO.
ECHO =============== Running test file: tsUpdatesADD_AH.cmd ======================
ECHO.
CALL "%DIRCMD%"\tsUpdatesADD_AH.cmd   

ECHO.
ECHO =============== Running test file: newBandWordTests_AH.cmd ==================
ECHO.
CALL "%DIRCMD%"\newBandWordTests_AH.cmd 

ECHO.
ECHO =============== Running test file: newdpasscals_AH.cmd ======================
ECHO.
CALL "%DIRCMD%"\newdpasscals_AH.cmd 

ECHO.
ECHO =============== Running test file: newowdpasscals_AH.cmd ====================
ECHO.
CALL "%DIRCMD%"\newowdpasscals_AH.cmd

ECHO.
ECHO =============== Running test file: newowspasscals_AH.cmd ====================
ECHO.
CALL "%DIRCMD%"\newowspasscals_AH.cmd

ECHO.
ECHO =============== Running test file: newregularcals_AH.cmd ====================
ECHO.
CALL "%DIRCMD%"\newregularcals_AH.cmd

ECHO.
ECHO =============== Running test file: newspasscals_AH.cmd ======================
ECHO.
CALL "%DIRCMD%"\newspasscals_AH.cmd

ECHO.
ECHO =============== Running test file: newOtherTests_AH.cmd =====================
ECHO.
CALL "%DIRCMD%"\newOtherTests_AH.cmd

ECHO.
ECHO =============== Running test file: newOtherTests2_AH.cmd ====================
ECHO.
CALL "%DIRCMD%"\newOtherTests2_AH.cmd

ECHO.
ECHO =============== Running test file: tsUpdatesDEL_AH.cmd ======================
ECHO.
CALL "%DIRCMD%"\tsUpdatesDEL_AH.cmd

:end
ENDLOCAL
exit /b 0