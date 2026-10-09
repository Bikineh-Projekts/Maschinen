FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./

RUN dotnet restore "Maschin/MaschinenDataein.csproj" -r linux-x64 -p:PublishReadyToRun=true

# ReadyToRun: Code wird vorab kompiliert -> deutlich schnellerer Start nach einem Neustart
RUN dotnet publish "Maschin/MaschinenDataein.csproj" -c Release -o /app/publish \
    -r linux-x64 --self-contained false --no-restore \
    -p:UseAppHost=false -p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "MaschinenDataein.dll"]
