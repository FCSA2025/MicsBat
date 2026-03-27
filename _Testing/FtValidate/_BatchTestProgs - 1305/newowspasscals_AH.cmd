@echo off
setlocal EnableDelayedExpansion EnableExtensions

REM Modified by Andrew Hulme on 22-Sep-2020.

REM prescribe the tag that is appended to the report file name.
set Tag=04-Oct-2020

REM prescribe the file path that the test report is written.
set LogFilePath=d:\Users\ahulme\temp\owspasscals%Tag%.txt

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

REM if the Log file already exists then delete it.
if exist %LogFilePath% del %LogFilePath%

REM if tempFile.txt already exists then delete it.
if exist tempFile.txt del tempFile.txt

REM identify the PDF files to be used and write their paths to a temporary 
REM file so that we can 'loop' over them using a 'for /f' construct.
echo %PDFdir%\owslat.txt              >> tempFile.txt
echo %PDFdir%\owslong.txt             >> tempFile.txt
echo %PDFdir%\owsgrnd.txt             >> tempFile.txt
echo %PDFdir%\owsacode.txt            >> tempFile.txt
echo %PDFdir%\owsaht.txt              >> tempFile.txt
echo %PDFdir%\owstxpwr.txt            >> tempFile.txt
echo %PDFdir%\owstxfreq.txt           >> tempFile.txt
echo %PDFdir%\owslata.txt             >> tempFile.txt
echo %PDFdir%\owslonga.txt            >> tempFile.txt
echo %PDFdir%\owsgrnda.txt            >> tempFile.txt
echo %PDFdir%\owsacodea.txt           >> tempFile.txt
echo %PDFdir%\owsahta.txt             >> tempFile.txt
echo %PDFdir%\owsrxfsla.txt           >> tempFile.txt
echo %PDFdir%\owstxfsla.txt           >> tempFile.txt
echo %PDFdir%\owstxpwra.txt           >> tempFile.txt
echo %PDFdir%\owstxfreqa.txt          >> tempFile.txt

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
	
	set /a count = !count! + 1
)

exit /b 0


