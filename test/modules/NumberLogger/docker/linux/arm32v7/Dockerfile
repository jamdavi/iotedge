ARG base_tag=1.0.6.13-linux-arm32v7
FROM azureiotedge/azureiotedge-module-base:${base_tag}

ARG EXE_DIR=.

ENV MODULE_NAME "NumberLogger.dll"

WORKDIR /app

COPY $EXE_DIR/ ./

USER moduleuser

ENTRYPOINT /usr/bin/dotnet NumberLogger.dll

