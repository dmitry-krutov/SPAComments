FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["FileService.sln", "FileService.sln"]
COPY ["src/FileService/FileService.csproj", "src/FileService/"]
COPY ["src/FileService.Contracts/FileService.Contracts.csproj", "src/FileService.Contracts/"]
COPY ["src/FileService.Communication/FileService.Communication.csproj", "src/FileService.Communication/"]

RUN dotnet restore "FileService.sln"

COPY . .
RUN dotnet publish "src/FileService/FileService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8081
COPY --from=build /app/publish .
EXPOSE 8081
ENTRYPOINT ["dotnet", "FileService.dll"]
