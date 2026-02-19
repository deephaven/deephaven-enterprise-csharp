FROM mcr.microsoft.com/dotnet/sdk:6.0
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

# cleanup any dross from local builds
RUN rm -rf obj
RUN rm -rf .vs
RUN rm -rf api/*.yml
RUN rm -rf api/.manifest

# setup packages
RUN dotnet restore SharedGenerator.sln
RUN dotnet restore DeephavenOpenAPI.sln

# build the core code and code generator
RUN dotnet build SharedGenerator.sln /p:Configuration=${CONFIGURATION} /p:Version=${VERSION}

# generate API classes
RUN /app/codegen.sh ${CONFIGURATION}

# build the API library
RUN dotnet build DeephavenOpenAPI.sln /p:Configuration=${CONFIGURATION} /p:Version=${VERSION}

# copy all nuget packages we want to export to /output
RUN mkdir -p /output
RUN cp /app/DeephavenOpenAPI/bin/${CONFIGURATION}/DeephavenOpenAPI.${VERSION}.nupkg /output

# run tests, transform output to junit format
RUN dotnet test DeephavenOpenAPI.sln --logger "junit;LogFilePath=/app/TEST-DeephavenOpenAPI.xml"

# 2.76.0 started breaking our builds due to some mysterious "server disconnected" message.
RUN dotnet tool update -g docfx --version 2.75.3
RUN $HOME/.dotnet/tools/docfx --version

RUN $HOME/.dotnet/tools/docfx

# tarball docs
RUN tar cvfz site.tgz _site/*

CMD ["tail", "-f", "/dev/null"]
