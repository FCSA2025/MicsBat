@echo off
setlocal EnableDelayedExpansion EnableExtensions

REM Modified by Andrew Hulme on 22-Sep-2020.

REM prescribe the tag that is appended to the report file name.
set Tag=04-Oct-2020

REM prescribe the file path that the test report is written.
set LogFilePath=d:\Users\ahulme\temp\bandwordtest%Tag%.txt

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
echo %PDFdir%\tstest08a.txt          >> tempFile.txt
echo %PDFdir%\tstest09a.txt          >> tempFile.txt
echo %PDFdir%\tstest1a.txt           >> tempFile.txt
echo %PDFdir%\tstest1b.txt           >> tempFile.txt
echo %PDFdir%\tstest1c.txt           >> tempFile.txt
echo %PDFdir%\tstest1d.txt           >> tempFile.txt
echo %PDFdir%\tstest1e.txt           >> tempFile.txt
echo %PDFdir%\tstest2a.txt           >> tempFile.txt
echo %PDFdir%\tstest2b.txt           >> tempFile.txt
echo %PDFdir%\tstest2c.txt           >> tempFile.txt
echo %PDFdir%\tstest2d.txt           >> tempFile.txt
echo %PDFdir%\tstest2e.txt           >> tempFile.txt
echo %PDFdir%\tstest2f.txt           >> tempFile.txt
echo %PDFdir%\tstest2g.txt           >> tempFile.txt
echo %PDFdir%\tstest2h.txt           >> tempFile.txt
echo %PDFdir%\tstest3a.txt           >> tempFile.txt
echo %PDFdir%\tstest3b.txt           >> tempFile.txt
echo %PDFdir%\tstest3c.txt           >> tempFile.txt
echo %PDFdir%\tstest3d.txt           >> tempFile.txt
echo %PDFdir%\tstest4a.txt           >> tempFile.txt
echo %PDFdir%\tstest5a.txt           >> tempFile.txt
echo %PDFdir%\tstest6a.txt           >> tempFile.txt
echo %PDFdir%\tstest6b.txt           >> tempFile.txt
echo %PDFdir%\tstest6c.txt           >> tempFile.txt
echo %PDFdir%\tstest6d.txt           >> tempFile.txt
echo %PDFdir%\tstest6e.txt           >> tempFile.txt
echo %PDFdir%\tstest7a.txt           >> tempFile.txt
echo %PDFdir%\tstest7b.txt           >> tempFile.txt
echo %PDFdir%\tstest8a.txt           >> tempFile.txt
echo %PDFdir%\tstest8b.txt           >> tempFile.txt
echo %PDFdir%\tstest10a.txt          >> tempFile.txt
echo %PDFdir%\tstest11a.txt          >> tempFile.txt
echo %PDFdir%\tstest11b.txt          >> tempFile.txt
echo %PDFdir%\tstest11c.txt          >> tempFile.txt
echo %PDFdir%\tstest12a.txt          >> tempFile.txt
echo %PDFdir%\tstest13a.txt          >> tempFile.txt
echo %PDFdir%\tstest14a.txt          >> tempFile.txt
echo %PDFdir%\tstest14b.txt          >> tempFile.txt
echo %PDFdir%\tstest14c.txt          >> tempFile.txt
echo %PDFdir%\tstest18a.txt          >> tempFile.txt
echo %PDFdir%\tstest18b.txt          >> tempFile.txt
echo %PDFdir%\tstest18c.txt          >> tempFile.txt
echo %PDFdir%\tstest19a.txt          >> tempFile.txt
echo %PDFdir%\tstest22a.txt          >> tempFile.txt
echo %PDFdir%\tstest24a.txt          >> tempFile.txt
echo %PDFdir%\tstest26a.txt          >> tempFile.txt
echo %PDFdir%\tstest28a.txt          >> tempFile.txt
echo %PDFdir%\tstest29a.txt          >> tempFile.txt
echo %PDFdir%\tstest31a.txt          >> tempFile.txt
echo %PDFdir%\tstest39a.txt          >> tempFile.txt
echo %PDFdir%\tstest78a.txt          >> tempFile.txt
echo %PDFdir%\bandwdaddreghop.txt    >> tempFile.txt
echo %PDFdir%\bandwddelreghop.txt    >> tempFile.txt
echo %PDFdir%\bandwdaddpashop.txt    >> tempFile.txt
echo %PDFdir%\bandwddelpashop.txt    >> tempFile.txt

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


