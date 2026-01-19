FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Fitz/Fitz.csproj", "Fitz/"]
COPY ["Fitz.Database/Fitz.Database.csproj", "Fitz.Database/"]
COPY ["Fitz.Metrics/Fitz.Metrics.csproj", "Fitz.Metrics/"]
RUN dotnet restore "Fitz/Fitz.csproj"
COPY . .
WORKDIR "/src/Fitz"
RUN dotnet build "Fitz.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/Fitz"
RUN dotnet publish "Fitz.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fitz.dll"]