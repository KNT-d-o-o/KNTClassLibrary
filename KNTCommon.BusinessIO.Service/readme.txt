
sc create KNTCommon.BusinessIO binPath= "D:\RAZVOJ\PUBLISH_IO\KNTCommon.BusinessIO.Service.exe"


Install service:
run cmd as Administrator:

Stop service command: SC STOP KNTCommon.BusinessIO
Delete service command: SC DELETE KNTCommon.BusinessIO

Create service command:
Debug:
SC CREATE KNTCommon.BusinessIO binPath= "D:\RAZVOJ\PUBLISH_IO\KNTCommon.BusinessIO.Service.exe" start=auto
Release:
SC CREATE KNTCommon.BusinessIO binPath="C:\KNTLeakTester\ServiceIO\KNTLeakTester.BusinessPLC.Service.exe" start=auto
SC CREATE KNTCommon.BusinessIO binPath="C:\PROGRAMS\ServiceIO\KNTLeakTester.BusinessPLC.Service.exe" start=auto

Start service command: SC START KNTCommon.BusinessIO