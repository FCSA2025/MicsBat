@echo off
setlocal EnableDelayedExpansion EnableExtensions

REM Designed by Andrew Hulme on 22-Sep-2020.

REM prescribe the date tag that is appended to the report file name.
set Tag=
call :GetDate_ddmmmyyyy Tag

REM prescribe the file path that the test report is written.
set LogFilePath=d:\Users\ahulme\temp\newtsadds%Tag%.txt

REM prescribe the paths to find FtImport, FtValidate, FtPrint and MtUpdate.
set FtImportDir=d:\prod\bin
set FtValidateDir=D:\Users\ahulme\MICS#\_bin\Release
set FtPrintDir=D:\Users\ahulme\MICS#\_bin\Release
set MtUpdateDir=d:\prod\bin

REM prescribe the SQL Server connection variables.
set MicsUser=hulme1
set password=14Coffee
set ProjCode=hulme1_0
set DBName=fcsa

REM prescribe the directory where the test PDFs are located.
set PDFdir=d:\Users\tsdatafiles

REM prescribe whether MDB table spoofing is to be used, or not.
set Spoofing=-s

set ERRORMSG=""					

REM if the Log file already exists then delete it.
if exist %LogFilePath% del %LogFilePath%

REM if tempFile.txt already exists then delete it.
if exist tempFile.txt del tempFile.txt

REM identify the PDF files to be used and write their paths to a temporary 
REM file so that we can 'loop' over them using a 'for /f' construct.
echo %PDFdir%\sadd.txt          >> tempFile.txt
echo %PDFdir%\owsadd.txt        >> tempFile.txt
echo %PDFdir%\dbleadd.txt       >> tempFile.txt
echo %PDFdir%\owdadd.txt        >> tempFile.txt
echo %PDFdir%\regadd.txt        >> tempFile.txt
echo %PDFdir%\regadddvant.txt   >> tempFile.txt
echo %PDFdir%\testelevadddb.txt >> tempFile.txt

echo.

REM calculate the number of PDF files by doing a line count of the
REM file tempFile.txt
set /a numPDFs = 0
for /f %%a in (tempFile.txt) do set /a numPDFs+=1

REM initialize a counter to echo to show progress.
set /a count = 1

REM this is the main loop over all PDF file paths.
for /f %%G IN (tempFile.txt) DO (
	
	echo Processing PDF !count! of %numPDFs%:      %%~nG
	
	%FtImportDir%\ftImport                  %DBName% %ProjCode% -f    %%~nG %%G  >> %LogFilePath%
	%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1 %%~nG      >> %LogFilePath%
	%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L     %%~nG      >> %LogFilePath%
	%MtUpdateDir%\mtUpdate     %Spoofing%   %DBName% %ProjCode%       %%~nG      >> %LogFilePath%
	
	set /a count = !count! + 1
) 

goto :eof

:GetDate_ddmmmyyyy
:: Returns a date string i.a.w. Claudia's convention, e.g. "14may2009"
set dayNow=%DATE:~7,2%
set month=%DATE:~4,2%
set yearNow=%DATE:~10,4%
if %month%==01 set monthNow=jan
if %month%==02 set monthNow=feb
if %month%==03 set monthNow=mar
if %month%==04 set monthNow=apr
if %month%==05 set monthNow=may
if %month%==06 set monthNow=jun
if %month%==07 set monthNow=jul
if %month%==08 set monthNow=aug
if %month%==09 set monthNow=sep
if %month%==10 set monthNow=oct
if %month%==11 set monthNow=nov
if %month%==12 set monthNow=dec
set dateNow=%dayNow%%monthNow%%yearNow%
set %1=%dateNow%
goto :eof    

exit /b 1

