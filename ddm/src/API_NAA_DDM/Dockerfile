FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG SOURCE_URL
ARG REPO_USER
ARG REPO_POSSWD
WORKDIR /src
COPY ["src/API_NAA_DDM/API_NAA_DDM.csproj", "src/API_NAA_DDM/"]
RUN dotnet nuget add source $SOURCE_URL --name nuget-repo --username $REPO_USER --password $REPO_POSSWD --store-password-in-clear-text
RUN dotnet restore "./src/API_NAA_DDM/API_NAA_DDM.csproj"
COPY . .
WORKDIR "/src/src/API_NAA_DDM"
RUN dotnet build "./API_NAA_DDM.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API_NAA_DDM.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API_NAA_DDM.dll"]
