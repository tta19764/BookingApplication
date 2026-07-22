FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BookingApp.Api/BookingApp.Api.csproj", "BookingApp.Api/"]
COPY ["BookingApp.Application/BookingApp.Application.csproj", "BookingApp.Application/"]
COPY ["BookingApp.Domain/BookingApp.Domain.csproj", "BookingApp.Domain/"]
COPY ["BookingApp.Infrastructure/BookingApp.Infrastructure.csproj", "BookingApp.Infrastructure/"]
RUN dotnet restore "BookingApp.Api/BookingApp.Api.csproj"
COPY . .
WORKDIR "/src/BookingApp.Api"
RUN dotnet build "./BookingApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BookingApp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingApp.Api.dll"]
