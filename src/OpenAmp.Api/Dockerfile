FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OpenAmp.sln ./
COPY src/OpenAmp.Domain/OpenAmp.Domain.csproj src/OpenAmp.Domain/
COPY src/OpenAmp.Application/OpenAmp.Application.csproj src/OpenAmp.Application/
COPY src/OpenAmp.Infrastructure/OpenAmp.Infrastructure.csproj src/OpenAmp.Infrastructure/
COPY src/OpenAmp.Api/OpenAmp.Api.csproj src/OpenAmp.Api/
RUN dotnet restore src/OpenAmp.Api/OpenAmp.Api.csproj

COPY src/ src/
RUN dotnet publish src/OpenAmp.Api/OpenAmp.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "OpenAmp.Api.dll"]
