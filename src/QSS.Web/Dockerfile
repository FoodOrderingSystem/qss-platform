FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/QSS.Web/QSS.Web.csproj", "src/QSS.Web/"]
COPY ["src/QSS.Application/QSS.Application.csproj", "src/QSS.Application/"]
COPY ["src/QSS.Domain/QSS.Domain.csproj", "src/QSS.Domain/"]
COPY ["src/QSS.Infrastructure/QSS.Infrastructure.csproj", "src/QSS.Infrastructure/"]
RUN dotnet restore "src/QSS.Web/QSS.Web.csproj"
COPY . .
WORKDIR "/src/src/QSS.Web"
RUN dotnet build "QSS.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QSS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QSS.Web.dll"]
