d:
cd D:\Users\fmda
set MicsUser=fwmda
set password=ddkviei223
set DBName=fcsa

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlat d:\Users\tsdatafiles\owdlat.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlat
d:\prod\bin\ftPrint fcsa FCSA20 L owdlat >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlong d:\Users\tsdatafiles\owdlong.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlong
d:\prod\bin\ftPrint fcsa FCSA20 L owdlong >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdgrnd d:\Users\tsdatafiles\owdgrnd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdgrnd
d:\prod\bin\ftPrint fcsa FCSA20 L owdgrnd >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdacode d:\Users\tsdatafiles\owdacode.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdacode
d:\prod\bin\ftPrint fcsa FCSA20 L owdacode >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdaht d:\Users\tsdatafiles\owdaht.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdaht
d:\prod\bin\ftPrint fcsa FCSA20 L owdaht >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxpwr d:\Users\tsdatafiles\owdtxpwr.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxpwr
d:\prod\bin\ftPrint fcsa FCSA20 L owdtxpwr >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxfreq d:\Users\tsdatafiles\owdtxfreq.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxfreq
d:\prod\bin\ftPrint fcsa FCSA20 L owdtxfreq >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlata d:\Users\tsdatafiles\owdlata.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlata
d:\prod\bin\ftPrint fcsa FCSA20 L owdlata >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlonga d:\Users\tsdatafiles\owdlonga.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdlonga
d:\prod\bin\ftPrint fcsa FCSA20 L owdlonga >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdgrnda d:\Users\tsdatafiles\owdgrnda.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdgrnda
d:\prod\bin\ftPrint fcsa FCSA20 L owdgrnda >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdacodea d:\Users\tsdatafiles\owdacodea.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdacodea
d:\prod\bin\ftPrint fcsa FCSA20 L owdacodea >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdahta d:\Users\tsdatafiles\owdahta.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdahta
d:\prod\bin\ftPrint fcsa FCSA20 L owdahta >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdrxfsla d:\Users\tsdatafiles\owdrxfsla.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdrxfsla
d:\prod\bin\ftPrint fcsa FCSA20 L owdrxfsla >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxfsla d:\Users\tsdatafiles\owdtxfsla.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxfsla
d:\prod\bin\ftPrint fcsa FCSA20 L owdtxfsla >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxpwra d:\Users\tsdatafiles\owdtxpwra.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxpwra
d:\prod\bin\ftPrint fcsa FCSA20 L owdtxpwra >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxfreqa d:\Users\tsdatafiles\owdtxfreqa.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owdpasscals14jan2020.txt owdtxfreqa
d:\prod\bin\ftPrint fcsa FCSA20 L owdtxfreqa >>d:\Users\fmda\ftValidateTests\owdpasscals14jan2020.txt
