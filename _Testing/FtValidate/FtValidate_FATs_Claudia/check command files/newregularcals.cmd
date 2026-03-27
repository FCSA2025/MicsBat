d:
cd d:\Users\fmda
set MicsUser=fwmda
set password=dkdkie8337
set DBName=fcsa

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt newoper1 d:\Users\tsdatafiles\newoper1.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt newoper1
d:\prod\bin\ftPrint fcsa FMDA20 L newoper1 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt newoper1
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt newoper1

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgelat d:\Users\tsdatafiles\chgelat.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgelat
d:\prod\bin\ftPrint fcsa FMDA20 L chgelat >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgelat
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgelat

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgelong d:\Users\tsdatafiles\chgelong.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgelong
d:\prod\bin\ftPrint fcsa FMDA20 L chgelong >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgelong
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgelong

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgegrnd d:\Users\tsdatafiles\chgegrnd.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgegrnd
d:\prod\bin\ftPrint fcsa FMDA20 L chgegrnd >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgegrnd
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgegrnd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgeacode d:\Users\tsdatafiles\chgeacode.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgeacode
d:\prod\bin\ftPrint fcsa FMDA20 L chgeacode >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgeacode
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgeacode


d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgeaht d:\Users\tsdatafiles\chgeaht.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgeaht
d:\prod\bin\ftPrint fcsa FMDA20 L chgeaht >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgeaht
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgeaht


d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgepss d:\Users\tsdatafiles\chgepss.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgepss
d:\prod\bin\ftPrint fcsa FMDA20 L chgepss >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgepss
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgepss

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgerxanum d:\Users\tsdatafiles\chgerxanum.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgerxanum
d:\prod\bin\ftPrint fcsa FMDA20 L chgerxanum >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgerxanum
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgerxanum

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgetxanum d:\Users\tsdatafiles\chgetxanum.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgetxanum
d:\prod\bin\ftPrint fcsa FMDA20 L chgetxanum >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgetxanum
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgetxanum

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgeause d:\Users\tsdatafiles\chgeause.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgeause
d:\prod\bin\ftPrint fcsa FMDA20 L chgeause >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgeause
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgeause

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgefreqtx d:\Users\tsdatafiles\chgefreqtx.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgefreqtx
d:\prod\bin\ftPrint fcsa FMDA20 L chgefreqtx >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgefreqtx
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgefreqtx

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgerxfsl d:\Users\tsdatafiles\chgerxfsl.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgerxfsl
d:\prod\bin\ftPrint fcsa FMDA20 L chgerxfsl >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgerxfsl
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgerxfsl

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgetxfsl d:\Users\tsdatafiles\chgetxfsl.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgetxfsl
d:\prod\bin\ftPrint fcsa FMDA20 L chgetxfsl >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgetxfsl
d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgetxfsl

REM d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgeaht2 d:\Users\tsdatafiles\chgeaht2.txt
REM d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgeaht2
REM d:\prod\bin\ftPrint fcsa FMDA20 L chgeaht2 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt


d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgedvaht d:\Users\tsdatafiles\chgedvaht.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgedvaht
d:\prod\bin\ftPrint fcsa FMDA20 L chgedvaht >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgedvaht
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgedvaht

REM chgedvaht does this
REM d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt adddivhigher d:\Users\tsdatafiles\adddivhigher.txt
REM d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt adddivhigher
REM d:\prod\bin\ftPrint fcsa FMDA20 L adddivhigher >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgedvacode d:\Users\tsdatafiles\chgedvacode.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt chgedvacode
d:\prod\bin\ftPrint fcsa FMDA20 L chgedvacode >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt chgedvacode
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt chgedvacode

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt addunusedant d:\Users\tsdatafiles\addunusedant.txt
d:\prod\bin\ftValidate fcsa FMDA20 -m:-1>>d:\users\fmda\ftValidateTests\regularcals14jan2020.txt addunusedant
d:\prod\bin\ftPrint fcsa FMDA20 L addunusedant >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt
REM d:\prod\bin\vch fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020.txt addunusedant
REM d:\prod\bin\vchnew fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\regularcals14jan2020c#.txt addunusedant
