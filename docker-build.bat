@ECHO OFF
REM an easy way to test the docker build without running from gradle
IF "%1"=="" GOTO Usage
IF "%2"=="" GOTO Usage

docker build --build-arg PROJECT_NAME=DeephavenOpenAPI --build-arg CONFIGURATION=%1 --build-arg VERSION=%2 .
GOTO Done

:Usage
echo usage: docker-build.bat ^<config^> ^<version^>

:Done