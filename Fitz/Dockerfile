FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETPLATFORM
WORKDIR /src

COPY ["Fitz.sln", "./"]
COPY ["Fitz/Fitz.csproj", "Fitz/"]
COPY ["Fitz.Database/Fitz.Database.csproj", "Fitz.Database/"]
COPY ["Fitz.Metrics/Fitz.Metrics.csproj", "Fitz.Metrics/"]

RUN dotnet restore "Fitz/Fitz.csproj"

COPY . .
WORKDIR "/src/Fitz"
RUN dotnet publish "Fitz.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ENV DOTNET_EnableWriteXorExecute=0
WORKDIR /app

COPY --from=build --chown=app:app /app/publish .

ENTRYPOINT ["dotnet", "Fitz.dll"]