FtImport -f fcsa hulme1_0 tafloutv2 "D:\Users\ahulme\MICS#\TestData\TS\PDFs\TsImportTests\tafloutv2.txt"
%MICSnative%\FtValidate fcsa hulme1_0  tafloutv2 > %TEMP%\FtValidateNATIVE.txt
FtImport -f fcsa hulme1_0 tafloutv2 "D:\Users\ahulme\MICS#\TestData\TS\PDFs\TsImportTests\tafloutv2.txt"
FtValidate fcsa hulme1_0 tafloutv2 > %TEMP%\FtValidate.txt