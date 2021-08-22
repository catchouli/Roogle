#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Roogle.RoogleSpiderFeeder/Roogle.RoogleSpiderFeeder.csproj", "Roogle.RoogleSpiderFeeder/"]
RUN dotnet restore "Roogle.RoogleSpiderFeeder/Roogle.RoogleSpiderFeeder.csproj"
COPY . .
WORKDIR "/src/Roogle.RoogleSpiderFeeder"
RUN dotnet build "Roogle.RoogleSpiderFeeder.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Roogle.RoogleSpiderFeeder.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Roogle.RoogleSpiderFeeder.dll"]
