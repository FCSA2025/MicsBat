d:
cd d:\Users\fmda
set MicsUser=fwmda
set password=ckckdie83

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delsadd d:\Users\tsdatafiles\delsadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delsadd
d:\prod\bin\ftPrint fcsa FMDA20 L delsadd >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delsadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delowsadd d:\Users\tsdatafiles\delowsadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delowsadd
d:\prod\bin\ftPrint fcsa FMDA20 L delowsadd >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delowsadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt deldbleadd d:\Users\tsdatafiles\deldbleadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt deldbleadd
d:\prod\bin\ftPrint fcsa FMDA20 L deldbleadd >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt deldbleadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delowdadd d:\Users\tsdatafiles\delowdadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delowdadd
d:\prod\bin\ftPrint fcsa FMDA20 L delowdadd >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delowdadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delregadd d:\Users\tsdatafiles\delregadd.txt
d:\prod\bin\ftValidate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delregadd
d:\prod\bin\ftPrint fcsa FMDA20 L delregadd >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt delregadd

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt regadddvant d:\Users\tsdatafiles\regadddvant.txt
d:\prod\bin\ftValidate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt regadddvant
d:\prod\bin\ftPrint fcsa FMDA20 L regadddvant >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt regadddvant

d:\prod\bin\ftImport fcsa FMDA20 -f >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt testelevadddb d:\Users\tsdatafiles\testelevadddb.txt
d:\prod\bin\ftValidate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt testelevadddb
d:\prod\bin\ftPrint fcsa FMDA20 L testelevadddb >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt
d:\prod\bin\mtUpdate fcsa FMDA20 >>d:\Users\fmda\ftValidateTests\deltsupdates14jan2020.txt testelevadddb

