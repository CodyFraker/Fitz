FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Fitz.sln", "./"]
COPY ["Fitz/Fitz.csproj", "Fitz/"]
COPY ["Fitz.Database/Fitz.Database.csproj", "Fitz.Database/"]
COPY ["Fitz.Metrics/Fitz.Metrics.csproj", "Fitz.Metrics/"]

RUN dotnet restore "Fitz/Fitz.csproj"

COPY . .
WORKDIR "/src/Fitz"
RUN dotnet build "Fitz.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY --from=build /src .
WORKDIR "/src/Fitz"
RUN dotnet restore "Fitz.csproj"
RUN dotnet publish "Fitz.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime:8.0 AS final
ENV DOTNET_EnableWriteXorExecute=0
WORKDIR /app

COPY --from=publish --chown=app:app /app/publish .

USER app

ENTRYPOINT ["dotnet", "Fitz.dll"]