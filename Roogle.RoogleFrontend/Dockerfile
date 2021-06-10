#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Roogle.RoogleFrontend/Roogle.RoogleFrontend.csproj", "Roogle.RoogleFrontend/"]
RUN dotnet restore "Roogle.RoogleFrontend/Roogle.RoogleFrontend.csproj"
COPY . .
WORKDIR "/src/Roogle.RoogleFrontend"
RUN dotnet build "Roogle.RoogleFrontend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Roogle.RoogleFrontend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Roogle.RoogleFrontend.dll"]