FROM andrewlock/dotnet-mono:2.1.504-sdk
ARG PROJECT_NAME
ARG CONFIGURATION
ARG VERSION
RUN echo "Building ${PROJECT_NAME} with configuration ${CONFIGURATION} and version ${VERSION}"
COPY . /app/
WORKDIR /app

# The https repo used to download mono has recently changed how they serve http versus https
# In order to fix our ability to use apt, we remove all apt sources, install apt-transport-https, then put those sources back
RUN find /etc/apt/sources.list.d -type f -exec bash -c 'mv $1 ${1}.bak' -- {} \; && \
 apt-get update && apt-get install -y apt-transport-https && \
 find /etc/apt/sources.list.d -type f -exec bash -c 'mv $1 ${1//.bak}' -- {} \;

# we need unzip for docfx package
RUN apt-get update && apt-get install unzip

# cleanup any dross from local builds
RUN rm -rf obj
RUN rm -rf .vs
RUN rm -rf api/*.yml
RUN rm -rf api/.manifest

# setup packages
RUN dotnet restore SharedGenerator.sln
RUN dotnet restore DeephavenOpenAPI.sln

# build the core code and code generator
RUN msbuild SharedGenerator.sln /p:Configuration=${CONFIGURATION} /p:Version=${VERSION}

# generate API classes
RUN /app/codegen.sh ${CONFIGURATION}

# build the API library
RUN msbuild DeephavenOpenAPI.sln /p:Configuration=${CONFIGURATION} /p:Version=${VERSION}

# copy all nuget packages we want to export to /output
RUN mkdir -p /output
RUN cp /app/DeephavenOpenAPI/bin/${CONFIGURATION}/DeephavenOpenAPI.${VERSION}.nupkg /output

# run tests, transform output to junit format
RUN dotnet test DeephavenOpenAPI.sln --logger "junit;LogFilePath=/app/TEST-DeephavenOpenAPI.xml"

# unzip / install docfx; we do the download in gradle to get it cached in jenkins
RUN mkdir docfx
RUN unzip docfx.zip -d docfx

# clean, generate docs and make a tarball
RUN mono docfx/docfx.exe

# tarball docs
RUN tar cvfz site.tgz _site/*

CMD ["tail", "-f", "/dev/null"]
