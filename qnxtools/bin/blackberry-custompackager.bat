@java -Djava.awt.headless=true -Xmx512M -cp "%~dp0\..\lib\EccpressoJDK15ECC.jar;%~dp0\..\lib\EccpressoAll.jar;%~dp0\..\lib\TrustpointAll.jar;%~dp0\..\lib\TrustpointJDK15.jar;%~dp0\..\lib\TrustpointProviders.jar;%~dp0\..\lib\BarPackager.jar;%~dp0\..\lib\BarSigner.jar;%~dp0\..\lib\BarDeploy.jar;%~dp0\..\lib\BarAir.jar"  com.qnx.bbt.custompackager.BarCustomPackager %*
