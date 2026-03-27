d:
cd d:\Users\fmda
set MicsUser=fwmda
set password=Dkddie333
set DBName=fcsa

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owslat d:\Users\tsdatafiles\owslat.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owslat
d:\prod\bin\ftPrint fcsa FMDA20 L owslat >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owslong d:\Users\tsdatafiles\owslong.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owslong
d:\prod\bin\ftPrint fcsa FMDA20 L owslong >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owsgrnd d:\Users\tsdatafiles\owsgrnd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owsgrnd
d:\prod\bin\ftPrint fcsa FMDA20 L owsgrnd >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owsacode d:\Users\tsdatafiles\owsacode.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owsacode
d:\prod\bin\ftPrint fcsa FMDA20 L owsacode >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owsaht d:\Users\tsdatafiles\owsaht.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owsaht
d:\prod\bin\ftPrint fcsa FMDA20 L owsaht >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxpwr d:\Users\tsdatafiles\owstxpwr.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxpwr
d:\prod\bin\ftPrint fcsa FMDA20 L owstxpwr >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxfreq d:\Users\tsdatafiles\owstxfreq.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxfreq
d:\prod\bin\ftPrint fcsa FMDA20 L owstxfreq >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owslata d:\Users\tsdatafiles\owslata.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owslata
d:\prod\bin\ftPrint fcsa FMDA20 L owslata >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owslonga d:\Users\tsdatafiles\owslonga.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owslonga
d:\prod\bin\ftPrint fcsa FMDA20 L owslonga >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owsgrnda d:\Users\tsdatafiles\owsgrnda.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owsgrnda
d:\prod\bin\ftPrint fcsa FMDA20 L owsgrnda >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owsacodea d:\Users\tsdatafiles\owsacodea.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owsacodea
d:\prod\bin\ftPrint fcsa FMDA20 L owsacodea >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owsahta d:\Users\tsdatafiles\owsahta.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owsahta
d:\prod\bin\ftPrint fcsa FMDA20 L owsahta >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owsrxfsla d:\Users\tsdatafiles\owsrxfsla.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owsrxfsla
d:\prod\bin\ftPrint fcsa FMDA20 L owsrxfsla >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxfsla d:\Users\tsdatafiles\owstxfsla.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxfsla
d:\prod\bin\ftPrint fcsa FMDA20 L owstxfsla >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxpwra d:\Users\tsdatafiles\owstxpwra.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxpwra
d:\prod\bin\ftPrint fcsa FMDA20 L owstxpwra >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxfreqa d:\Users\tsdatafiles\owstxfreqa.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\users\fmda\ftValidateTests\owspasscals14jan2020.txt owstxfreqa
d:\prod\bin\ftPrint fcsa FMDA20 L owstxfreqa >>d:\Users\fmda\ftValidateTests\owspasscals14jan2020.txt


