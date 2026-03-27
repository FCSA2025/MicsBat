@ECHO OFF
REM make all parameters defined here local in scope
REM i.e. the value of system environment parameters will not be overwritten.
REM Just in case, enable command shell extensions.
SETLOCAL EnableExtensions		 

:Process
REM =================================================================================================

set MicsUser=hulme1
set password=bananas
set DBNAME=fcsa



@ECHO ON

set PROJECT=hulme1_0
set PDFDIR=D:\Users\ahulme\MICS#\TestData\FtValidate
set TEMP=D:\Users\ahulme\temp

set SRCDIR_A=D:\Users\ahulme\FCSA_Native_Progs\bin20180406
set OUTFILE_A=%TEMP%\ZULU_A.txt
del %OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% sadd %PDFDIR%\sadd.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% sadd
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L sadd >>%OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% owsadd %PDFDIR%\owsadd.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% owsadd
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L owsadd >>%OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% dbleadd %PDFDIR%\dbleadd.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% dbleadd
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L dbleadd >>%OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% owdadd %PDFDIR%\owdadd.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% owdadd
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L owdadd >>%OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% regadd %PDFDIR%\regadd.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% regadd
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L regadd >>%OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% regadddvant %PDFDIR%\regadddvant.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% regadddvant
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L regadddvant >>%OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% testelevadddb %PDFDIR%\testelevadddb.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% testelevadddb
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L testelevadddb >>%OUTFILE_A%

%SRCDIR_A%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_A% tselev6200 %PDFDIR%\tselev6200.txt
%SRCDIR_A%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_A% tselev6200
%SRCDIR_A%\ftPrint %DBNAME% %PROJECT% L tselev6200 >>%OUTFILE_A%

REM -----------------------

set SRCDIR_B=D:\Users\ahulme\MICS#\_bin\Release
set OUTFILE_B=%TEMP%\ZULU_B.txt
del %OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% sadd %PDFDIR%\sadd.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% sadd
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L sadd >>%OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% owsadd %PDFDIR%\owsadd.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% owsadd
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L owsadd >>%OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% dbleadd %PDFDIR%\dbleadd.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% dbleadd
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L dbleadd >>%OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% owdadd %PDFDIR%\owdadd.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% owdadd
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L owdadd >>%OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% regadd %PDFDIR%\regadd.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% regadd
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L regadd >>%OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% regadddvant %PDFDIR%\regadddvant.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% regadddvant
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L regadddvant >>%OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% testelevadddb %PDFDIR%\testelevadddb.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% testelevadddb
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L testelevadddb >>%OUTFILE_B%

%SRCDIR_B%\ftImport %DBNAME% %PROJECT% -f >>%OUTFILE_B% tselev6200 %PDFDIR%\tselev6200.txt
%SRCDIR_B%\ftValidate %DBNAME% %PROJECT% -m:-1 >>%OUTFILE_B% tselev6200
%SRCDIR_B%\ftPrint %DBNAME% %PROJECT% L tselev6200 >>%OUTFILE_B%

@ECHO OFF

REM =================================================================================================

GOTO:end

:SyntaxError
ECHO Syntax Error: %ERRORMSG%
ECHO.
ECHO Usage: TOUCH [[drive:]dirPath] filex [/R]
ECHO.
ECHO        [drive:]dirPath : Absolute path to a directory, or a 
ECHO                          relative path using "." and/or ".."
ECHO.
ECHO        filex           : File names to be touched. May contain 
ECHO                          'wild cards', used to discriminate the 
ECHO						  files to be touched.
ECHO                          E.g. foo.txt, *.bat, *, ?i*.* etc.
ECHO.
ECHO        /R              : Recursive descent to include all subdirectories.  
ECHO.
ECHO Example:  touch  c:\Users\Andrew\Documents  *.doc  /R       
GOTO:end 

:End
ENDLOCAL
GOTO:eof

