FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OpenAmp.sln ./
COPY src/OpenAmp.Application/OpenAmp.Application.csproj src/OpenAmp.Application/
COPY src/OpenAmp.Domain/OpenAmp.Domain.csproj src/OpenAmp.Domain/
COPY src/OpenAmp.Worker/OpenAmp.Worker.csproj src/OpenAmp.Worker/
RUN dotnet restore src/OpenAmp.Worker/OpenAmp.Worker.csproj

COPY src/ src/
RUN dotnet publish src/OpenAmp.Worker/OpenAmp.Worker.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OpenAmp.Worker.dll"]
