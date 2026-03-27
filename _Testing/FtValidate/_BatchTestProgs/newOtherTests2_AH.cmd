@echo OFF
setlocal EnableDelayedExpansion EnableExtensions

REM Modified by Andrew Hulme on 22-Sep-2020.

REM prescribe the date tag that is appended to the report file name.
set Tag=
call :GetDate_ddmmmyyyy Tag

REM prescribe the file path that the test report is written.
set LogFilePath=d:\Users\ahulme\temp\othertests2%Tag%.txt

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
echo %PDFdir%\licence1.txt          >> tempFile.txt
echo %PDFdir%\licence2.txt          >> tempFile.txt
echo %PDFdir%\aunkn31.txt           >> tempFile.txt
echo %PDFdir%\aunkn34.txt           >> tempFile.txt
echo %PDFdir%\ftest43.txt           >> tempFile.txt
echo %PDFdir%\ftest44.txt           >> tempFile.txt
echo %PDFdir%\ftest45.txt           >> tempFile.txt
echo %PDFdir%\ftest46.txt           >> tempFile.txt
echo %PDFdir%\testerr3.txt          >> tempFile.txt
echo %PDFdir%\b071030av2.txt        >> tempFile.txt
echo %PDFdir%\b080512at3.txt        >> tempFile.txt
echo %PDFdir%\b080918at3.txt        >> tempFile.txt

echo.

REM calculate the number of PDF files by doing a line count of the
REM file tempFile.txt
set /a numPDFs = 0
for /f %%a in (tempFile.txt) do set /a numPDFs+=1

REM initialize a counter to echo to show progress.
set /a count = 1

REM this is the main loop over all PDF file paths.
for /f %%G IN (tempFile.txt) DO (

    REM  Almost all of the filenames appearing in a dir listing of "D:\Users\tsdatafiles" are in lower case.
	REM  A very small number of the filenames in "D:\Users\tsdatafiles" are UPPER CASE.
	REM  e.g. AUNKN31.txt and AUNKN34.txt
	REM  When the filepath string is read from tempFile.txt, %%~nG RETAINS ITS CASE !
	REM  This causes havoc w.r.t. the "if" statements below and when comparing results using WinMerge.
	REM  We will define a local variable, set it to %%~nG and then convert its value to lower case.	
	set PDFlowerCase=%%~nG
	call :LoCase PDFlowerCase
	
	echo Processing PDFlowerCase !count! of %numPDFs%:      %%~nG
	
	%FtImportDir%\ftImport                                 %DBName% %ProjCode% -f  !PDFlowerCase!  %%G  >> %LogFilePath%
	
	if %%~nG==AUNKN31 %FtPrintDir%\ftPrint    %Spoofing%   %DBName% %ProjCode% L   !PDFlowerCase!       >> %LogFilePath%
	if %%~nG==AUNKN34 %FtPrintDir%\ftPrint    %Spoofing%   %DBName% %ProjCode% L   !PDFlowerCase!       >> %LogFilePath%
	if %%~nG==ftest43 %FtPrintDir%\ftPrint    %Spoofing%   %DBName% %ProjCode% L   !PDFlowerCase!       >> %LogFilePath%
	
	%FtValidateDir%\ftValidate %Spoofing%   %DBName% %ProjCode% -m:-1              !PDFlowerCase!       >> %LogFilePath%
	%FtPrintDir%\ftPrint       %Spoofing%   %DBName% %ProjCode% L                  !PDFlowerCase!       >> %LogFilePath%
	
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

:LoCase
:: Subroutine to convert a variable VALUE to all lower case.
:: The argument for this subroutine is the variable NAME.
FOR %%i IN ("A=a" "B=b" "C=c" "D=d" "E=e" "F=f" "G=g" "H=h" "I=i" "J=j" "K=k" "L=l" "M=m" "N=n" "O=o" "P=p" "Q=q" "R=r" "S=s" "T=t" "U=u" "V=v" "W=w" "X=x" "Y=y" "Z=z") DO CALL SET "%1=%%%1:%%~i%%"
GOTO:EOF

:UpCase
:: Subroutine to convert a variable VALUE to all UPPER CASE.
:: The argument for this subroutine is the variable NAME.
FOR %%i IN ("a=A" "b=B" "c=C" "d=D" "e=E" "f=F" "g=G" "h=H" "i=I" "j=J" "k=K" "l=L" "m=M" "n=N" "o=O" "p=P" "q=Q" "r=R" "s=S" "t=T" "u=U" "v=V" "w=W" "x=X" "y=Y" "z=Z") DO CALL SET "%1=%%%1:%%~i%%"
GOTO:EOF

:TCase
:: Subroutine to convert a variable VALUE to Title Case.
:: The argument for this subroutine is the variable NAME.
FOR %%i IN (" a= A" " b= B" " c= C" " d= D" " e= E" " f= F" " g= G" " h= H" " i= I" " j= J" " k= K" " l= L" " m= M" " n= N" " o= O" " p= P" " q= Q" " r= R" " s= S" " t= T" " u= U" " v= V" " w= W" " x= X" " y= Y" " z= Z") DO CALL SET "%1=%%%1:%%~i%%"
GOTO:EOF

endlocal
exit /b 0


