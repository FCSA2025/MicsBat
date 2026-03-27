d:
cd d:\Users\fmda
set MicsUser=fwmda
set password=dkdkeee839
set DBName=fcsa

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevdel d:\Users\tsdatafiles\testelevdel.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevdel
d:\prod\bin\ftPrint fcsa fmda0 L testelevdel >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vch fcsa FMDA0 testelevdel >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevdel >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\mtUpdate fcsa fmda0 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevdel

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevadd d:\Users\tsdatafiles\testelevadd.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevadd
d:\prod\bin\ftPrint fcsa fmda0 L testelevadd >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vch fcsa FMDA0 testelevadd >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevadd >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevadddb d:\Users\tsdatafiles\testelevadddb.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevadddb
d:\prod\bin\ftPrint fcsa fmda0 L testelevadddb >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vch fcsa FMDA0 testelevadddb >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevadddb >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\mtUpdate fcsa fmda0 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevadddb

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglea d:\Users\tsdatafiles\testelevanglea.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglea
D:\prod\bin\vch fcsa FMDA0 testelevanglea >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevanglea >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\ftPrint fcsa fmda0 L testelevanglea >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglea2 d:\Users\tsdatafiles\testelevanglea2.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglea2
D:\prod\bin\vch fcsa FMDA0 testelevanglea2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevanglea2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\ftPrint fcsa fmda0 L testelevanglea2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangleb d:\Users\tsdatafiles\testelevangleb.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangleb
D:\prod\bin\vch fcsa FMDA0 testelevangleb >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevangleb >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\ftPrint fcsa fmda0 L testelevangleb >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

REM d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangleb2 d:\Users\tsdatafiles\testelevangleb2.txt
REM d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangleb2
REM D:\prod\bin\vch fcsa FMDA0 testelevangleb2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
REM D:\prod\bin\vchnew fcsa FMDA0 testelevangleb2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
REM d:\prod\bin\ftPrint fcsa fmda0 L testelevangleb2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglec d:\Users\tsdatafiles\testelevanglec.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglec
D:\prod\bin\vch fcsa FMDA0 testelevanglec >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevanglec >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\ftPrint fcsa fmda0 L testelevanglec >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangled d:\Users\tsdatafiles\testelevangled.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangled
D:\prod\bin\vch fcsa FMDA0 testelevangled >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevangled >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\ftPrint fcsa fmda0 L testelevangled >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangled2 d:\Users\tsdatafiles\testelevangled2.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevangled2
D:\prod\bin\vch fcsa FMDA0 testelevangled2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevangled2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\ftPrint fcsa fmda0 L testelevangled2 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglee d:\Users\tsdatafiles\testelevanglee.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevanglee
D:\prod\bin\vch fcsa FMDA0 testelevanglee >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
D:\prod\bin\vchnew fcsa FMDA0 testelevanglee >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c#.txt
d:\prod\bin\ftPrint fcsa fmda0 L testelevanglee >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt

d:\prod\bin\ftImport fcsa fmda0 -f >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevdel d:\Users\tsdatafiles\testelevdel.txt
d:\prod\bin\ftValidate fcsa fmda0 -m:-1 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevdel
d:\prod\bin\ftPrint fcsa fmda0 L testelevdel >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt
d:\prod\bin\mtUpdate fcsa fmda0 >>d:\Users\fmda\ftValidateTests\elevtests04nov2019c++.txt testelevdel




