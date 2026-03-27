@echo off
setlocal EnableDelayedExpansion EnableExtensions

REM Modified by Andrew Hulme on 22-Sep-2020.
d:

set MicsUser=hulme1
set password=14Coffee
set ProjCode=hulme1_0
set DBName=fcsa
set TodaysDate=26-Sep-2020

set PDFdir=d:\Users\tsdatafiles
REM set LogFilePath=d:\Users\foad\ftValidateTests\tsUpdateADD14nov2017.txt
set LogFilePath=d:\Users\ahulme\temp\elevtests%TodaysDate%.txt

REM if the Log file already exists then delete it.
if exist %LogFilePath% del %LogFilePath%

REM if tempFile.txt already exists then delete it.
if exist tempFile.txt del tempFile.txt

REM identify the PDF files to be used and write their paths to a temporary 
REM file so that we can 'loop' over them using a 'for /f' construct.
echo %PDFdir%\testelevdel.txt           >> tempFile.txt
echo %PDFdir%\testelevadd.txt           >> tempFile.txt
echo %PDFdir%\testelevadddb.txt         >> tempFile.txt
echo %PDFdir%\testelevanglea.txt        >> tempFile.txt
echo %PDFdir%\testelevanglea2.txt       >> tempFile.txt
echo %PDFdir%\testelevangleb.txt        >> tempFile.txt
REM echo %PDFdir%\testelevangleb2.txt       >> tempFile.txt
echo %PDFdir%\testelevanglec.txt        >> tempFile.txt
echo %PDFdir%\testelevangled.txt        >> tempFile.txt
echo %PDFdir%\testelevangled2.txt       >> tempFile.txt
echo %PDFdir%\testelevanglee.txt        >> tempFile.txt
echo %PDFdir%\testelevdel.txt           >> tempFile.txt

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
	
	d:\prod\bin\ftImport %DBName% %ProjCode% -f %%~nG %%G >> %LogFilePath%
	d:\prod\bin\ftValidate %DBName% %ProjCode% -m:-1 %%~nG >> %LogFilePath%
	d:\prod\bin\ftPrint %DBName% %ProjCode% L %%~nG >> %LogFilePath%
	
	set /a count = !count! + 1
)

exit /b 0


