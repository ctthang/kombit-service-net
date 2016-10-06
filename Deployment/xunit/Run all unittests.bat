REM Modify the path below according to absolute path of the xunit folder
cd "C:\inetpub\Kombit.Samples\xunit"
REM Remove "REM " of the line below if tests and xunit are place somewhere in the D drive
REM D:

xunit.console "..\Tests\Kombit.Samples.BasicPrivilegeProfileParser.Tests\Kombit.Samples.BasicPrivilegeProfileParser.Tests.dll" -quiet -html "..\Tests\BasicPrivilegeProfileParser.html" 
xunit.console ..\Tests\Kombit.Samples.CHTestSigningService.Tests\Kombit.Samples.CHTestSigningService.Tests.dll -quiet -html "..\Tests\CHTestSigningService.html" 
xunit.console ..\Tests\Kombit.Samples.RestProvisioningService.Tests\Kombit.Samples.RestProvisioningService.Tests.dll -quiet -html "..\Tests\RestProvisioningService.html" 
xunit.console ..\Tests\Kombit.Samples.RestProvisioningServiceModel.Tests\Kombit.Samples.RestProvisioningServiceModel.Tests.dll -quiet -html "..\Tests\RestProvisioningServiceModel.html" 
xunit.console ..\Tests\Kombit.Samples.STS.Tests\Kombit.Samples.STS.Tests.dll -quiet -html "..\Tests\STS.html" 
xunit.console ..\Tests\Kombit.Samples.STSTestSigningService.Tests\Kombit.Samples.STSTestSigningService.Tests.dll -quiet -html "..\Tests\STSTestSigningService.html" 
xunit.console ..\Tests\Kombit.Samples.Consumer\Kombit.Samples.Consumer.dll -quiet -html "..\Tests\Consumer.html" 
pause
