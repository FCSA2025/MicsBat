@ECHO OFF

REM :  check for 3 command-line arguments.
SET validArgs=
IF [%1]==[] SET validArgs=1
IF [%2]==[] SET validArgs=1
IF [%3]==[] SET validArgs=1
IF NOT [%4]==[] SET validArgs=1
IF defined validArgs (  SET ERRORMSG=Too few or too many command-line arguments.
						goto :SyntaxError
					  )
REM :  so we have 3 command-line arguments; make the assignments.
SET MasterDateTag=%1
SET ResultsDir=%2
SET ResultsDateTag=%3

REM :  validate that MasterDateTag is one of the recognized dates.
SET MasterDir=
IF [%MasterDateTag%]==[01jun2016] (SET MasterDir=D:\Users\ahulme\MICS#\_Testing\FtValidate\_FtValidateMasterTestReports_01jun2016)
IF [%MasterDateTag%]==[20jun2019] (SET MasterDir=D:\Users\ahulme\MICS#\_Testing\FtValidate\_FtValidateMasterTestReports_20jun2019)
IF [%MasterDateTag%]==[19sep2019] (SET MasterDir=D:\Users\ahulme\MICS#\_Testing\FtValidate\_FtValidateMasterTestReports_19sep2019)
IF [%MasterDateTag%]==[14jan2020] (SET MasterDir=D:\Users\ahulme\MICS#\_Testing\FtValidate\_FtValidateMasterTestReports_14jan2020)
IF NOT DEFINED MasterDir ( SET ERRORMSG=%MasterDateTag% is not a recognized master report date.
						   goto :SyntaxError	
                          )
						  
REM :  validate that ResultsDir is a path to a directory.
IF NOT EXIST %ResultsDir%\nul ( SET ERRORMSG=Invalid directory path: %ResultsDir%
						        goto :SyntaxError	
                               )
SET testName_1=newtsadds
SET testName_2=bandwordtest
SET testName_3=dpasscals
SET testName_4=owdpasscals
SET testName_5=owspasscals
SET testName_6=regularcals
SET testName_7=spasscals
SET testName_8=OtherTests
SET testName_9=OtherTests2
SET testName_10=deltsupdates
	
SET "result_1=FAIL ^<^<^<"
SET "result_2=FAIL ^<^<^<"
SET "result_3=FAIL ^<^<^<"
SET "result_4=FAIL ^<^<^<"
SET "result_5=FAIL ^<^<^<"
SET "result_6=FAIL ^<^<^<"
SET "result_7=FAIL ^<^<^<"
SET "result_8=FAIL ^<^<^<"
SET "result_9=FAIL ^<^<^<"
SET "result_10=FAIL ^<^<^<"

SET WinMergeUexePath=D:\tools\WinMerge\WinMergeU.exe

SET WinMergeCL_1=
SET WinMergeCL_2=
SET WinMergeCL_3=
SET WinMergeCL_4=
SET WinMergeCL_5=
SET WinMergeCL_6=
SET WinMergeCL_7=
SET WinMergeCL_8=
SET WinMergeCL_9=
SET WinMergeCL_10=

SET diagnostic_1=
SET diagnostic_2=
SET diagnostic_3=
SET diagnostic_4=
SET diagnostic_5=
SET diagnostic_6=
SET diagnostic_7=
SET diagnostic_8=
SET diagnostic_9=
SET diagnostic_10=

ECHO.
ECHO.
ECHO ====== FtValidate Auto-Test: Master vs New Reports  =============================================
ECHO.
ECHO        Master Reports Date:       %MasterDateTag% 
ECHO        New    Reports Date:       %ResultsDateTag%
ECHO        New Reports in Directory:  %ResultsDir%
ECHO.
ECHO.
ECHO        Test#    Report          Result
ECHO        -----    ------          ------
ECHO.

REM ======================================= [1] =======================================================

SET left="%MasterDir%\%testName_1%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_1%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_1=PASS

SET WinMergeCL_1=%WinMergeUexePath% %left% %right%

ECHO        1.       %testName_1%       %result_1%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [2] =======================================================

SET left="%MasterDir%\%testName_2%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_2%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_2=PASS

SET WinMergeCL_2=%WinMergeUexePath% %left% %right%

ECHO        2.       %testName_2%    %result_2%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [3] =======================================================

SET left="%MasterDir%\%testName_3%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_3%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_3=PASS

SET WinMergeCL_3=%WinMergeUexePath% %left% %right%

ECHO        3.       %testName_3%       %result_3%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [4] =======================================================

SET left="%MasterDir%\%testName_4%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_4%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_4=PASS

SET WinMergeCL_4=%WinMergeUexePath% %left% %right%

ECHO        4.       %testName_4%     %result_4%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [5] =======================================================

SET left="%MasterDir%\%testName_5%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_5%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_5=PASS

SET WinMergeCL_5=%WinMergeUexePath% %left% %right%

ECHO        5.       %testName_5%     %result_5%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [6] =======================================================

SET left="%MasterDir%\%testName_6%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_6%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_6=PASS

SET WinMergeCL_6=%WinMergeUexePath% %left% %right%

ECHO        6.       %testName_6%     %result_6%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [7] =======================================================

SET left="%MasterDir%\%testName_7%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_7%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_7=PASS

SET WinMergeCL_7=%WinMergeUexePath% %left% %right%

ECHO        7.       %testName_7%       %result_7%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [8] =======================================================

SET left="%MasterDir%\%testName_8%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_8%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_8=PASS

SET WinMergeCL_8=%WinMergeUexePath% %left% %right%

ECHO        8.       %testName_8%      %result_8%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [9] =======================================================

SET left="%MasterDir%\%testName_9%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_9%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_9=PASS

SET WinMergeCL_9=%WinMergeUexePath% %left% %right%

ECHO        9.       %testName_9%     %result_9%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

REM ======================================= [10] ======================================================

SET left="%MasterDir%\%testName_10%%MasterDateTag%.txt"
SET right="%ResultsDir%\%testName_10%%ResultsDateTag%.txt"

Tools CompareResults %left% %right% -E

IF [%errorlevel%]==[0] SET result_10=PASS

SET WinMergeCL_10=%WinMergeUexePath% %left% %right%

ECHO        10.      %testName_10%    %result_10%
ECHO.

FOR /F "delims=" %%G IN (%USERPROFILE%\CompareResultsExport.txt) DO ECHO %%G

ECHO ================== Launch WinMerge or Exit?  =================================================
ECHO.

SET /P userInput=To launch Winmerge for a specific test enter its number [1 to 10] or 0 to exit: 

IF [%userInput%]==[1] START %WinMergeCL_1%
IF [%userInput%]==[2] START %WinMergeCL_2%
IF [%userInput%]==[3] START %WinMergeCL_3%
IF [%userInput%]==[4] START %WinMergeCL_4%
IF [%userInput%]==[5] START %WinMergeCL_5%
IF [%userInput%]==[6] START %WinMergeCL_6%
IF [%userInput%]==[7] START %WinMergeCL_7%
IF [%userInput%]==[8] START %WinMergeCL_8%
IF [%userInput%]==[9] START %WinMergeCL_9%
IF [%userInput%]==[10] START %WinMergeCL_10%

goto end:

:SyntaxError
ECHO.
ECHO Syntax Error: %ERRORMSG%
ECHO.
ECHO Usage: 
ECHO =====
ECHO.
ECHO  RunAllComparisons  ^<masterReportsDate^>  ^<newReportsDir^>  ^<newReportsDate^>
ECHO.
ECHO           masterReportsDate   : 01jun2016, 20jun2019, 19sep2019 or 14jan2020
ECHO.
ECHO           newReportsDir       : directory containing new reports.
ECHO.
ECHO           newReportsDate      : the date tag of the new reports.
ECHO.
ECHO Example:  RunAllComparisons  14jan2020  d:\users\ahulme\temp  13nov2020  
ECHO.  
GOTO :end 

:End
ENDLOCAL
exit /b 0