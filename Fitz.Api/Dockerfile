ARG BUILDPLATFORM=linux/amd64
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Fitz.Api/Fitz.Api.csproj", "Fitz.Api/"]
COPY ["Fitz/Fitz.csproj", "Fitz/"]
RUN dotnet restore "Fitz.Api/Fitz.Api.csproj"
COPY . .
WORKDIR "/src/Fitz.Api"
RUN dotnet build "Fitz.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/Fitz.Api"
RUN dotnet publish "Fitz.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fitz.Api.dll"]
