d:
cd\Users\fmda
set MicsUser=fwmda
set password=vkife2339
set DBName=fcsa

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt sadd d:\Users\tsdatafiles\sadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt sadd
d:\prod\bin\ftPrint fcsa FMDA20 L sadd >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt sadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt owsadd d:\Users\tsdatafiles\owsadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt owsadd
d:\prod\bin\ftPrint fcsa FMDA20 L owsadd >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt owsadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt dbleadd d:\Users\tsdatafiles\dbleadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt dbleadd
d:\prod\bin\ftPrint fcsa FMDA20 L dbleadd >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt dbleadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt owdadd d:\Users\tsdatafiles\owdadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt owdadd
d:\prod\bin\ftPrint fcsa FMDA20 L owdadd >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt owdadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt regadd d:\Users\tsdatafiles\regadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt regadd
d:\prod\bin\ftPrint fcsa FMDA20 L regadd >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt regadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt regadddvant d:\Users\tsdatafiles\regadddvant.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt regadddvant
d:\prod\bin\ftPrint fcsa FMDA20 L regadddvant >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt regadddvant

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt testelevadddb d:\Users\tsdatafiles\testelevadddb.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt testelevadddb
d:\prod\bin\ftPrint fcsa FMDA20 L testelevadddb >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\newtsadds14jan2020.txt testelevadddb

