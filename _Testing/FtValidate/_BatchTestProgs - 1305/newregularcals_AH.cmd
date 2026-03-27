@echo off
setlocal EnableDelayedExpansion EnableExtensions

REM Modified by Andrew Hulme on 22-Sep-2020.

REM prescribe the tag that is appended to the report file name.
set Tag=04-Oct-2020

REM prescribe the file path that the test report is written.
set LogFilePath=d:\Users\ahulme\temp\regularcals%Tag%.txt

REM prescribe the paths to find FtImport, FtValidate, FtPrint and MtUpdate.
set FtImportDir=d:\prod\bin
set FtValidateDir=D:\Users\ahulme\MICS#\_bin\Release
set FtPrintDir=D:\Users\ahulme\MICS#\_bin\Release
set MtUpdateDir=d:\prod\bin
set VchDir=d:\prod\bin

REM prescribe the SQL Server connection variables.
set MicsUser=hulme1
set password=14Coffee
set ProjCode=hulme1_0
set DBName=fcsa

REM prescribe whether MDB table spoofing is to be used, or not.
set Spoofing=-s	

REM prescribe the directory where the test PDFs are located.
set PDFdir=d:\Users\tsdatafiles

REM if the Log files already exists then delete it.
if exist %LogFilePath%   del %LogFilePath%

REM if tempFile.txt already exists then delete it.
if exist tempFile.txt del tempFile.txt

REM identify the PDF files to be used and write their paths to a temporary 
REM file so that we can 'loop' over them using a 'for /f' construct.
echo %PDFdir%\newoper1.txt              >> tempFile.txt
echo %PDFdir%\chgelat.txt               >> tempFile.txt
echo %PDFdir%\chgelong.txt              >> tempFile.txt
echo %PDFdir%\chgegrnd.txt              >> tempFile.txt
echo %PDFdir%\chgeacode.txt             >> tempFile.txt
echo %PDFdir%\chgeaht.txt               >> tempFile.txt
echo %PDFdir%\chgepss.txt               >> tempFile.txt
echo %PDFdir%\chgerxanum.txt            >> tempFile.txt
echo %PDFdir%\chgetxanum.txt            >> tempFile.txt
echo %PDFdir%\chgeause.txt              >> tempFile.txt
echo %PDFdir%\chgefreqtx.txt            >> tempFile.txt
echo %PDFdir%\chgerxfsl.txt             >> tempFile.txt
echo %PDFdir%\chgetxfsl.txt             >> tempFile.txt
REM echo %PDFdir%\chgeaht2.txt          >> tempFile.txt
echo %PDFdir%\chgedvaht.txt             >> tempFile.txt
REM echo %PDFdir%\adddivhigher.txt      >> tempFile.txt
echo %PDFdir%\chgedvacode.txt           >> tempFile.txt
echo %PDFdir%\addunusedant.txt          >> tempFile.txt

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
	
	if %%~nG==chgetxanum %VchDir%\Vch %DBName% %ProjCode%   chgetxanum   >> %LogFilePath%
	if %%~nG==chgetxfsl  %VchDir%\Vch %DBName% %ProjCode%   chgetxfsl    >> %LogFilePath%
	
	set /a count = !count! + 1
)

exit /b 0
REM===========================================================================

echo Processing PDF:  newoper1.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   newoper1 %PDFdir%\newoper1.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  newoper1   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L newoper1 >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   newoper1   >> %LogFilePath%

echo Processing PDF:  chgelat.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgelat %PDFdir%\chgelat.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgelat   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgelat >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgelat   >> %LogFilePath%

echo Processing PDF:  chgelong.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgelong %PDFdir%\chgelong.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgelong   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgelong >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgelong   >> %LogFilePath%

echo Processing PDF:  chgegrnd.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgegrnd %PDFdir%\chgegrnd.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgegrnd   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgegrnd >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgegrnd   >> %LogFilePath%

echo Processing PDF:  chgeacode.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeacode %PDFdir%\chgeacode.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeacode   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeacode >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgeacode   >> %LogFilePath%

echo Processing PDF:  chgeaht.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeaht %PDFdir%\chgeaht.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeaht   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeaht >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgeaht   >> %LogFilePath%

echo Processing PDF:  chgepss.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgepss %PDFdir%\chgepss.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgepss   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgepss >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgepss   >> %LogFilePath%

echo Processing PDF:  chgerxanum.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgerxanum %PDFdir%\chgerxanum.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgerxanum   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgerxanum >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgerxanum   >> %LogFilePath%

echo Processing PDF:  chgetxanum.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgetxanum %PDFdir%\chgetxanum.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgetxanum   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgetxanum >>%LogFilePath%
%VchDir%\Vch %DBName% %ProjCode%   chgetxanum   >> %LogFilePath%

echo Processing PDF:  chgeause.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeause %PDFdir%\chgeause.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeause   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeause >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgeause   >> %LogFilePath%

echo Processing PDF:  chgefreqtx.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgefreqtx %PDFdir%\chgefreqtx.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgefreqtx   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgefreqtx >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgefreqtx   >> %LogFilePath%

echo Processing PDF:  chgerxfsl.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgerxfsl %PDFdir%\chgerxfsl.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgerxfsl   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgerxfsl >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgerxfsl   >> %LogFilePath%

echo Processing PDF:  chgetxfsl.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgetxfsl %PDFdir%\chgetxfsl.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgetxfsl   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgetxfsl >>%LogFilePath%
%VchDir%\Vch %DBName% %ProjCode%   chgetxfsl   >> %LogFilePath%

REM echo Processing PDF:  chgeaht2.txt
REM %FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeaht2 %PDFdir%\chgeaht2.txt   >> %LogFilePath%
REM %FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeaht2   >> %LogFilePath%
REM %FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeaht2 >>%LogFilePath%

echo Processing PDF:  chgedvaht.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgedvaht %PDFdir%\chgedvaht.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgedvaht   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgedvaht >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgedvaht   >> %LogFilePath%

REM echo Processing PDF:  adddivhigher.txt
REM chgedvaht does this
REM %FtImportDir%\ftImport                  %DBName% %ProjCode% -f   adddivhigher %PDFdir%\adddivhigher.txt   >> %LogFilePath%
REM %FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  adddivhigher   >> %LogFilePath%
REM %FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L adddivhigher >>%LogFilePath%

echo Processing PDF:  chgedvacode.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgedvacode %PDFdir%\chgedvacode.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgedvacode   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgedvacode >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   chgedvacode   >> %LogFilePath%

echo Processing PDF:  addunusedant.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   addunusedant %PDFdir%\addunusedant.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  addunusedant   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L addunusedant >>%LogFilePath%
REM %VchDir%\Vch %DBName% %ProjCode%   addunusedant   >> %LogFilePath%
