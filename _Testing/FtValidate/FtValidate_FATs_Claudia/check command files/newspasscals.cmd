d:
cd d:\Users\fmda
set MicsUser=fwmda
set password=xkxksww233
set DBName=fcsa

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt slat d:\Users\tsdatafiles\slat.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt slat
d:\prod\bin\ftPrint fcsa FMDA20 L slat >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt slong d:\Users\tsdatafiles\slong.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt slong
d:\prod\bin\ftPrint fcsa FMDA20 L slong >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt sgrnd d:\Users\tsdatafiles\sgrnd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt sgrnd
d:\prod\bin\ftPrint fcsa FMDA20 L sgrnd >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt sacode d:\Users\tsdatafiles\sacode.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt sacode
d:\prod\bin\ftPrint fcsa FMDA20 L sacode >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

REM not enough height change to affect rxpowers use saht2
REM d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt saht d:\Users\tsdatafiles\saht.txt
REM d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt saht
REM d:\prod\bin\ftPrint fcsa FMDA20 L saht >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt saht2 d:\Users\tsdatafiles\saht2.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt saht2
d:\prod\bin\ftPrint fcsa FMDA20 L saht2 >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt stxpwr d:\Users\tsdatafiles\stxpwr.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt stxpwr
d:\prod\bin\ftPrint fcsa FMDA20 L stxpwr >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt stxfreq d:\Users\tsdatafiles\stxfreq.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt stxfreq
d:\prod\bin\ftPrint fcsa FMDA20 L stxfreq >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt slata d:\Users\tsdatafiles\slata.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt slata
d:\prod\bin\ftPrint fcsa FMDA20 L slata >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt slonga d:\Users\tsdatafiles\slonga.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt slonga
d:\prod\bin\ftPrint fcsa FMDA20 L slonga >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt sgrnda d:\Users\tsdatafiles\sgrnda.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt sgrnda
d:\prod\bin\ftPrint fcsa FMDA20 L sgrnda >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt sacodea d:\Users\tsdatafiles\sacodea.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt sacodea
d:\prod\bin\ftPrint fcsa FMDA20 L sacodea >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

REM Use sahtac only
REM d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt sahta d:\Users\tsdatafiles\sahta.txt
REM d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt sahta
REM d:\prod\bin\ftPrint fcsa FMDA20 L sahta >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt sahtac d:\Users\tsdatafiles\sahtac.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt sahtac
d:\prod\bin\ftPrint fcsa FMDA20 L sahtac >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt srxfslav2 d:\Users\tsdatafiles\srxfslav2.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt srxfslav2
d:\prod\bin\ftPrint fcsa FMDA20 L srxfslav2 >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt stxfslav2 d:\Users\tsdatafiles\stxfslav2.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt stxfslav2
d:\prod\bin\ftPrint fcsa FMDA20 L stxfslav2 >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt stxpwrav2 d:\Users\tsdatafiles\stxpwrav2.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt stxpwrav2
d:\prod\bin\ftPrint fcsa FMDA20 L stxpwrav2 >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt stxfreqav2 d:\Users\tsdatafiles\stxfreqav2.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt stxfreqav2
d:\prod\bin\ftPrint fcsa FMDA20 L stxfreqav2 >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

REM adds nothing different from previous test
REM d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt stxfreqa2 d:\Users\tsdatafiles\stxfreqa2.txt
REM d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\spasscals14jan2020.txt stxfreqa2
REM d:\prod\bin\ftPrint fcsa FMDA20 L stxfreqa2 >>d:\Users\fmda\ftValidateTests\spasscals14jan2020.txt

