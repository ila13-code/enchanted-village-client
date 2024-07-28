FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

RUN dotnet tool install -g docfx
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY . ./

CMD ["docfx", "docfx.json"]