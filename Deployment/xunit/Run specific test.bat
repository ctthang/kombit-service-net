REM Modify the path below according to absolute path of the xunit folder
cd "c:\Data\Github\kombit-service-net\Deployment\xunit\"
REM Remove "REM " of the line below if tests and xunit are place somewhere in the D drive
REM D:

xunit.console ..\Tests\Kombit.Samples.Consumer\Kombit.Samples.Consumer.dll -method Kombit.Samples.Consumer.Consumer.SendRstAndThenExecuteServiceServiceSuccessfully -quiet -html "..\Tests\Consumer.html" 
pause
