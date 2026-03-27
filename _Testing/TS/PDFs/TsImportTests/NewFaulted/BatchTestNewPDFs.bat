REM Batch file to test FtImport using multiple PDFs.

REM ===== Set the Windows Environment variables =====

SET MicsUser=fwmda
SET password=xkxksww233
SET DBName=fcsa

REM ===== Set the working directory into which the .log file is written. =====

CD "d:\users\tests ftImport"

REM ===== To shorten the pathnames define a variable for the   =====
REM ====== directory containing the PDFs used for the testing. =====

SET PdfDir="D:\_drop\FtImport 20181213\FtImport Testing PDFs (new-faulted)"

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKduplicateAnte  %PdfDir%\AKduplicateAnte.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKduplicateAnte  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKextraField  %PdfDir%\AKextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKfieldsTooLong  %PdfDir%\AKfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKinvalidDate  %PdfDir%\AKinvalidDate.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKinvalidDate  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKinvalidTime  %PdfDir%\AKinvalidTime.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKinvalidTime  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKnoFirstComma  %PdfDir%\AKnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKnoLastComma  %PdfDir%\AKnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKnoMandatories  %PdfDir%\AKnoMandatories.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKnoMandatories  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKok  %PdfDir%\AKok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AKoutOfSequence  %PdfDir%\AKoutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AKoutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  AOextraField  %PdfDir%\AOextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AOextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AOfieldsTooLong  %PdfDir%\AOfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AOfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AOnoFirstComma  %PdfDir%\AOnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AOnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AOnoLastComma  %PdfDir%\AOnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AOnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AOnoRecord  %PdfDir%\AOnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AOnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AOok  %PdfDir%\AOok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AOok  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  AQextraField  %PdfDir%\AQextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AQextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AQfieldsTooLong  %PdfDir%\AQfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AQfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AQinvalidDate  %PdfDir%\AQinvalidDate.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AQinvalidDate  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AQnoFinalComma  %PdfDir%\AQnoFinalComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AQnoFinalComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AQnoFirstComma  %PdfDir%\AQnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AQnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AQok  %PdfDir%\AQok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AQok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  AQoutOfSequence  %PdfDir%\AQoutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  AQoutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKduplicateChan  %PdfDir%\CKduplicateChan.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKduplicateChan  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKextraField  %PdfDir%\CKextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKfieldsTooLong  %PdfDir%\CKfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKinvalidDate  %PdfDir%\CKinvalidDate.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKinvalidDate  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKinvalidTime  %PdfDir%\CKinvalidTime.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKinvalidTime  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKnoFirstComma  %PdfDir%\CKnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKnoLastComma  %PdfDir%\CKnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKnoMandatories  %PdfDir%\CKnoMandatories.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKnoMandatories  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKnoRecord  %PdfDir%\CKnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CKok  %PdfDir%\CKok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CKok  >>  AllFtImportResults.log

%PdfDir%CKoutOfSequence.txt


d:\prod\bin\FtImport  fcsa  FMDA20  -f  COfieldsTooLong  %PdfDir%\COfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  COfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  COnoFirstField  %PdfDir%\COnoFirstField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  COnoFirstField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  COnoLastField  %PdfDir%\COnoLastField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  COnoLastField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  COnoRecord  %PdfDir%\COnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  COnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  COok  %PdfDir%\COok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  COok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  COoutOfSequence  %PdfDir%\COoutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  COoutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  CQextraField  %PdfDir%\CQextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CQextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CQfieldsTooLong  %PdfDir%\CQfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CQfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CQnoFirstComma  %PdfDir%\CQnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CQnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CQnoLastComma  %PdfDir%\CQnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CQnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CQnoRecord  %PdfDir%\CQnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CQnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CQok  %PdfDir%\CQok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CQok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CQoutOfSequence  %PdfDir%\CQoutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CQoutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  CRextraField  %PdfDir%\CRextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CRextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CRfieldsTooLong  %PdfDir%\CRfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CRfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CRnoFirstComma  %PdfDir%\CRnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CRnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CRnoLastComma  %PdfDir%\CRnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CRnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CRnoRecord  %PdfDir%\CRnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CRnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CRok  %PdfDir%\CRok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CRok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CRoutOfSequence  %PdfDir%\CRoutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CRoutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  CTextraField  %PdfDir%\CTextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CTextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CTfieldsTooLong  %PdfDir%\CTfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CTfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CTnoFirstComma  %PdfDir%\CTnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CTnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CTnoLastComma  %PdfDir%\CTnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CTnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CTnoRecord  %PdfDir%\CTnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CTnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CTnoRecord  %PdfDir%\CTnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CTnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CTok  %PdfDir%\CTok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CTok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  CToutOfSequence  %PdfDir%\CToutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  CToutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  GKduplicateRecords  %PdfDir%\GKduplicateRecords.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  GKduplicateRecords  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  GKextraFields  %PdfDir%\GKextraFields.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  GKextraFields  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  GKfieldsTooLong  %PdfDir%\GKfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  GKfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  GKnoFields  %PdfDir%\GKnoFields.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  GKnoFields  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  GKnoMandatories  %PdfDir%\GKnoMandatories.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  GKnoMandatories  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  GKok  %PdfDir%\GKok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  GKok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  GKoutOfSequence  %PdfDir%\GKoutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  GKoutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDextraComma  %PdfDir%\SDextraComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDextraComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDfieldsTooLong  %PdfDir%\SDfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDinvalidDate  %PdfDir%\SDinvalidDate.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDinvalidDate  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDnoFirstComma  %PdfDir%\SDnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDnoLastComma  %PdfDir%\SDnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDnoRecord  %PdfDir%\SDnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDok  %PdfDir%\SDok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDok  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SDoutOfSequence  %PdfDir%\SDoutOfSequence.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SDoutOfSequence  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKduplicateSite  %PdfDir%\SKduplicateSite.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKduplicateSite  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKextraField  %PdfDir%\SKextraField.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKextraField  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKfieldsTooLong  %PdfDir%\SKfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKinvalidDate  %PdfDir%\SKinvalidDate.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKinvalidDate  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKinvalidGrndHt  %PdfDir%\SKinvalidGrndHt.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKinvalidGrndHt  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKinvalidLatit  %PdfDir%\SKinvalidLatit.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKinvalidLatit  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKinvalidLongit  %PdfDir%\SKinvalidLongit.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKinvalidLongit  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKinvalidTime  %PdfDir%\SKinvalidTime.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKinvalidTime  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKnoMandatories  %PdfDir%\SKnoMandatories.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKnoMandatories  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  SKok  %PdfDir%\SKok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  SKok  >>  AllFtImportResults.log


d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTextraComma  %PdfDir%\TTextraComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTextraComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTfieldsTooLong  %PdfDir%\TTfieldsTooLong.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTfieldsTooLong  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTinvalidDate  %PdfDir%\TTinvalidDate.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTinvalidDate  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTinvalidTime  %PdfDir%\TTinvalidTime.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTinvalidTime  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTnoFirstComma  %PdfDir%\TTnoFirstComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTnoFirstComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTnoLastComma  %PdfDir%\TTnoLastComma.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTnoLastComma  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTnoRecord  %PdfDir%\TTnoRecord.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTnoRecord  >>  AllFtImportResults.log

d:\prod\bin\FtImport  fcsa  FMDA20  -f  TTok  %PdfDir%\TTok.txt  >>  AllFtImportResults.log
d:\prod\bin\FtPrint  fcsa  FMDA20  TTok  >>  AllFtImportResults.log

%PdfDir%\TToutOfSequence.txt