FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Flavian.WebAPI/Flavian.WebAPI.csproj", "Flavian.WebAPI/"]
COPY ["Flavian.Application/Flavian.Application.csproj", "Flavian.Application/"]
COPY ["Flavian.Domain/Flavian.Domain.csproj", "Flavian.Domain/"]
COPY ["Flavian.Persistence/Flavian.Persistence.csproj", "Flavian.Persistence/"]
COPY ["Flavian.Shared/Flavian.Shared.csproj", "Flavian.Shared/"]
COPY ["Flavian.Configuration/Flavian.Configuration.csproj", "Flavian.Configuration/"]
COPY ["Flavian.Infrastructure/Flavian.Infrastructure.csproj", "Flavian.Infrastructure/"]
RUN dotnet restore "./Flavian.WebAPI/Flavian.WebAPI.csproj"
COPY . .
WORKDIR "/src/Flavian.WebAPI"
RUN dotnet build "./Flavian.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Flavian.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS="http://+:80"
ENTRYPOINT ["dotnet", "Flavian.WebAPI.dll"]
