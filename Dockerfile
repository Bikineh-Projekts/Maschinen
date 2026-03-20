FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution/project files
COPY *.sln ./
COPY Maschin/*.csproj ./Maschin/
RUN dotnet restore

# Copy everything and build
COPY . ./
RUN dotnet publish Maschin/Maschin.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "MaschinenDataein.dll"]
