FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookingApp.Services.Web/BookingApp.Services.Web.csproj", "BookingApp.Services.Web/"]
COPY ["BookingApp.Bll/BookingApp.Bll.csproj", "BookingApp.Bll/"]
COPY ["BookingApp.Bll.Common/BookingApp.Bll.Common.csproj", "BookingApp.Bll.Common/"]
COPY ["BookingApp.Dal.SqlServerRepositories/BookingApp.Dal.SqlServerRepositories.csproj", "BookingApp.Dal.SqlServerRepositories/"]
RUN dotnet restore "BookingApp.Services.Web/BookingApp.Services.Web.csproj"
COPY . .
WORKDIR "/src/BookingApp.Services.Web"
RUN dotnet build "./BookingApp.Services.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookingApp.Services.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingApp.Services.Web.dll"]
