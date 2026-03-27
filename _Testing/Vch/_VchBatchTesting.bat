@ECHO OFF

ECHO.
ECHO regadd.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_regadd_output.txt -Ti  regadd
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 regadd -df > %TEMP%\Vch_regadd_results_GOLDEN.txt
Vch fcsa hulme1_0 regadd -df > %TEMP%\Vch_regadd_results.txt

ECHO.
ECHO batchawana.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_batchawana_output.txt -Ti  batchawana
ECHO FtValidate exit Code is %errorlevel%
Vch fcsa hulme1_0 batchawana -df > %TEMP%\Vch_batchawana_results.txt
%MICSnative%\Vch fcsa hulme1_0 batchawana -df > %TEMP%\Vch_batchawana_results_GOLDEN.txt

ECHO.
ECHO vff327.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_vff327_output.txt -Ti  vff327
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 vff327 -df > %TEMP%\Vch_vff327_results_GOLDEN.txt
Vch fcsa hulme1_0 vff327 -df > %TEMP%\Vch_vff327_results.txt

ECHO.
ECHO chrxpwr.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_chrxpwr_output.txt -Ti  chrxpwr
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 chrxpwr -df > %TEMP%\Vch_chrxpwr_results_GOLDEN.txt
Vch fcsa hulme1_0 chrxpwr -df > %TEMP%\Vch_chrxpwr_results.txt

ECHO.
ECHO fccwaelev.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_fccwaelev_output.txt -Ti  fccwaelev
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 fccwaelev -df > %TEMP%\Vch_fccwaelev_results_GOLDEN.txt
Vch fcsa hulme1_0 fccwaelev -df > %TEMP%\Vch_fccwaelev_results.txt

ECHO.
ECHO tafl35i13a.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_tafl35i13a_output.txt -Ti  tafl35i13a
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 tafl35i13a -df > %TEMP%\Vch_tafl35i13a_results_GOLDEN.txt
Vch fcsa hulme1_0 tafl35i13a -df > %TEMP%\Vch_tafl35i13a_results.txt

ECHO.
ECHO fccmirxpwr.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_fccmirxpwr_output.txt -Ti  fccmirxpwr
%MICSnative%\Vch fcsa hulme1_0 fccmirxpwr -df > %TEMP%\Vch_fccmirxpwr_results_GOLDEN.txt
Vch fcsa hulme1_0 fccmirxpwr -df > %TEMP%\Vch_fccmirxpwr_results.txt

ECHO.
ECHO fccnyrxpwr.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_fccnyrxpwr_output.txt -Ti  fccnyrxpwr
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 fccnyrxpwr -df > %TEMP%\Vch_fccnyrxpwr_results_GOLDEN.txt
Vch fcsa hulme1_0 fccnyrxpwr -df > %TEMP%\Vch_fccnyrxpwr_results.txt

ECHO.
ECHO fccwavchrxpwr.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_fccwavchrxpwr_output.txt -Ti  fccwavchrxpwr
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 fccwavchrxpwr -df > %TEMP%\Vch_fccwavchrxpwr_results_GOLDEN.txt
Vch fcsa hulme1_0 fccwavchrxpwr -df > %TEMP%\Vch_fccwavchrxpwr_results.txt

ECHO.
ECHO dgrnd.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_dgrnd_output.txt -Ti  dgrnd
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 dgrnd -df > %TEMP%\Vch_dgrnd_results_GOLDEN.txt
Vch fcsa hulme1_0 dgrnd -df > %TEMP%\Vch_dgrnd_results.txt

ECHO.
ECHO bchy2vi1027v1b.txt
FtValidate fcsa HULME_1 -V -o%TEMP%\FtValidate_bchy2vi1027v1b_output.txt -Ti  bchy2vi1027v1b
ECHO FtValidate exit Code is %errorlevel%
%MICSnative%\Vch fcsa hulme1_0 bchy2vi1027v1b -df > %TEMP%\Vch_bchy2vi1027v1b_results_GOLDEN.txt
Vch fcsa hulme1_0 bchy2vi1027v1b -df > %TEMP%\Vch_bchy2vi1027v1b_results.txt

:End
ENDLOCAL
GOTO:eof