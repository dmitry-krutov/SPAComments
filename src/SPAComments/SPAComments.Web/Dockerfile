# Build and publish SPAComments modular monolith
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["SPAComments.sln", "SPAComments.sln"]
COPY ["src/SPAComments/SPAComments.Web/SPAComments.Web.csproj", "src/SPAComments/SPAComments.Web/"]
COPY ["src/SPAComments/CaptchaModule/SPAComments.CaptchaModule.Application/SPAComments.CaptchaModule.Application.csproj", "src/SPAComments/CaptchaModule/SPAComments.CaptchaModule.Application/"]
COPY ["src/SPAComments/CaptchaModule/SPAComments.CaptchaModule.Infrastructure/SPAComments.CaptchaModule.Infrastructure.csproj", "src/SPAComments/CaptchaModule/SPAComments.CaptchaModule.Infrastructure/"]
COPY ["src/SPAComments/CaptchaModule/SPAComments.CaptchaModule.Presentation/SPAComments.CaptchaModule.Presentation.csproj", "src/SPAComments/CaptchaModule/SPAComments.CaptchaModule.Presentation/"]
COPY ["src/SPAComments/CommentsModule/SPAComments.CommentsModule.Application/SPAComments.CommentsModule.Application.csproj", "src/SPAComments/CommentsModule/SPAComments.CommentsModule.Application/"]
COPY ["src/SPAComments/CommentsModule/SPAComments.CommentsModule.Domain/SPAComments.CommentsModule.Domain.csproj", "src/SPAComments/CommentsModule/SPAComments.CommentsModule.Domain/"]
COPY ["src/SPAComments/CommentsModule/SPAComments.CommentsModule.Infrastructure/SPAComments.CommentsModule.Infrastructure.csproj", "src/SPAComments/CommentsModule/SPAComments.CommentsModule.Infrastructure/"]
COPY ["src/SPAComments/CommentsModule/SPAComments.CommentsModule.Presentation/SPAComments.CommentsModule.Presentation.csproj", "src/SPAComments/CommentsModule/SPAComments.CommentsModule.Presentation/"]
COPY ["src/SPAComments.FileService/src/FileService.Communication/FileService.Communication.csproj", "src/SPAComments.FileService/src/FileService.Communication/"]
COPY ["src/SPAComments.FileService/src/FileService.Contracts/FileService.Contracts.csproj", "src/SPAComments.FileService/src/FileService.Contracts/"]

RUN dotnet restore "SPAComments.sln"

COPY . .

RUN dotnet publish "src/SPAComments/SPAComments.Web/SPAComments.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SPAComments.Web.dll"]
