FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/QSS.API/QSS.API.csproj", "src/QSS.API/"]
COPY ["src/QSS.Application/QSS.Application.csproj", "src/QSS.Application/"]
COPY ["src/QSS.Domain/QSS.Domain.csproj", "src/QSS.Domain/"]
COPY ["src/QSS.Infrastructure/QSS.Infrastructure.csproj", "src/QSS.Infrastructure/"]
RUN dotnet restore "src/QSS.API/QSS.API.csproj"
COPY . .
WORKDIR "/src/src/QSS.API"
RUN dotnet build "QSS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QSS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/data /app/wwwroot/uploads
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QSS.API.dll"]
