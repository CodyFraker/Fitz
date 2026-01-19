FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Fitz.sln", "./"]
COPY ["Fitz.Api/Fitz.Api.csproj", "Fitz.Api/"]
COPY ["Fitz/Fitz.csproj", "Fitz/"]
COPY ["Fitz.Database/Fitz.Database.csproj", "Fitz.Database/"]
COPY ["Fitz.Metrics/Fitz.Metrics.csproj", "Fitz.Metrics/"]
COPY ["Fitz.Seeds/Fitz.Seeds.csproj", "Fitz.Seeds/"]

RUN dotnet restore "Fitz.Api/Fitz.Api.csproj"

COPY . .
WORKDIR "/src/Fitz.Api"
RUN dotnet build "Fitz.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY --from=build /src .
WORKDIR "/src/Fitz.Api"
RUN dotnet restore "Fitz.Api.csproj"
RUN dotnet publish "Fitz.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish --chown=app:app /app/publish .

USER app

ENTRYPOINT ["dotnet", "Fitz.Api.dll"]
