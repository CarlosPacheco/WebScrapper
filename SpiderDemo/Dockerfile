#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["SpiderDemo/SpiderDemo.csproj", "SpiderDemo/"]
COPY ["MisterSpider/MisterSpider.csproj", "MisterSpider/"]
RUN dotnet restore "SpiderDemo/SpiderDemo.csproj"
COPY . .
WORKDIR "/src/SpiderDemo"
RUN dotnet build "SpiderDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SpiderDemo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpiderDemo.dll"]

# Install cultures (same approach as Alpine SDK image)
RUN apk add --no-cache icu-libs libgcc libintl

# Disable the invariant mode (set in base image)
# ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
# https://docs.microsoft.com/en-us/dotnet/core/runtime-config/globalization
# true - In globalization-invariant mode, don't allow creation of any culture except the invariant culture.false - Allow creation of any culture.
# ENV DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY=false