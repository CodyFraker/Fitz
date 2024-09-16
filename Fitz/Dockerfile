FROM mcr.microsoft.com/dotnet/aspnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/aspnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Fitz.csproj", "Fitz/"]
RUN dotnet restore "./Fitz/Fitz.csproj"
WORKDIR "/src/Fitz"
COPY . .
RUN dotnet build "./Fitz.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fitz.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fitz.dll"]