#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["opcua-to-prometheus.csproj", ""]
RUN dotnet restore "./opcua-to-prometheus.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "opcua-to-prometheus.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "opcua-to-prometheus.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "opcua-to-prometheus.dll"]