@echo off
setlocal EnableDelayedExpansion EnableExtensions

REM Modified by Andrew Hulme on 22-Sep-2020.

REM prescribe the tag that is appended to the report file name.
set Tag=04-Oct-2020

REM prescribe the file path that the test report is written.
set LogFilePath=d:\Users\ahulme\temp\regularcals%Tag%.txt
set LogFilePathC#=d:\Users\ahulme\temp\regularcalsC#%Tag%.txt

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

REM if the Log files already exists then delete it.
if exist %LogFilePath%   del %LogFilePath%
if exist %LogFilePathC#% del %LogFilePathC#%

echo.

echo Processing PDF:  newoper1.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   newoper1 %PDFdir%\newoper1.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  newoper1   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L newoper1 >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   newoper1   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   newoper1   >> %LogFilePathC#%

echo Processing PDF:  chgelat.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgelat %PDFdir%\chgelat.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgelat   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgelat >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgelat   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgelat   >> %LogFilePathC#%

echo Processing PDF:  chgelong.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgelong %PDFdir%\chgelong.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgelong   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgelong >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgelong   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgelong   >> %LogFilePathC#%

echo Processing PDF:  chgegrnd.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgegrnd %PDFdir%\chgegrnd.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgegrnd   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgegrnd >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgegrnd   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgegrnd   >> %LogFilePathC#%

echo Processing PDF:  chgeacode.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeacode %PDFdir%\chgeacode.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeacode   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeacode >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgeacode   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgeacode   >> %LogFilePathC#%

echo Processing PDF:  chgeaht.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeaht %PDFdir%\chgeaht.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeaht   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeaht >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgeaht   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgeaht   >> %LogFilePathC#%

echo Processing PDF:  chgepss.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgepss %PDFdir%\chgepss.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgepss   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgepss >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgepss   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgepss   >> %LogFilePathC#%

echo Processing PDF:  chgerxanum.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgerxanum %PDFdir%\chgerxanum.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgerxanum   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgerxanum >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgerxanum   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgerxanum   >> %LogFilePathC#%

echo Processing PDF:  chgetxanum.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgetxanum %PDFdir%\chgetxanum.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgetxanum   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgetxanum >>%LogFilePath%
d:\prod\bin\vch %DBName% %ProjCode%   chgetxanum   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgetxanum   >> %LogFilePathC#%

echo Processing PDF:  chgeause.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeause %PDFdir%\chgeause.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeause   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeause >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgeause   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgeause   >> %LogFilePathC#%

echo Processing PDF:  chgefreqtx.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgefreqtx %PDFdir%\chgefreqtx.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgefreqtx   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgefreqtx >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgefreqtx   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgefreqtx   >> %LogFilePathC#%

echo Processing PDF:  chgerxfsl.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgerxfsl %PDFdir%\chgerxfsl.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgerxfsl   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgerxfsl >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgerxfsl   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgerxfsl   >> %LogFilePathC#%

echo Processing PDF:  chgetxfsl.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgetxfsl %PDFdir%\chgetxfsl.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgetxfsl   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgetxfsl >>%LogFilePath%
d:\prod\bin\vch %DBName% %ProjCode%   chgetxfsl   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgetxfsl   >> %LogFilePathC#%

REM echo Processing PDF:  chgeaht2.txt
REM %FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgeaht2 %PDFdir%\chgeaht2.txt   >> %LogFilePath%
REM %FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgeaht2   >> %LogFilePath%
REM %FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgeaht2 >>%LogFilePath%

echo Processing PDF:  chgedvaht.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgedvaht %PDFdir%\chgedvaht.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgedvaht   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgedvaht >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgedvaht   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgedvaht   >> %LogFilePathC#%

REM echo Processing PDF:  adddivhigher.txt
REM chgedvaht does this
REM %FtImportDir%\ftImport                  %DBName% %ProjCode% -f   adddivhigher %PDFdir%\adddivhigher.txt   >> %LogFilePath%
REM %FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  adddivhigher   >> %LogFilePath%
REM %FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L adddivhigher >>%LogFilePath%

echo Processing PDF:  chgedvacode.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   chgedvacode %PDFdir%\chgedvacode.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  chgedvacode   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L chgedvacode >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   chgedvacode   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   chgedvacode   >> %LogFilePathC#%

echo Processing PDF:  addunusedant.txt
%FtImportDir%\ftImport                  %DBName% %ProjCode% -f   addunusedant %PDFdir%\addunusedant.txt   >> %LogFilePath%
%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1  addunusedant   >> %LogFilePath%
%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L addunusedant >>%LogFilePath%
REM d:\prod\bin\vch %DBName% %ProjCode%   addunusedant   >> %LogFilePath%
REM d:\prod\bin\vchnew %DBName% %ProjCode%   addunusedant   >> %LogFilePathC#%
