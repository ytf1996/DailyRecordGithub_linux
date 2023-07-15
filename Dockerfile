#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

COPY ["MyNetCore/MyNetCore.csproj", "MyNetCore/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "MyNetCore/MyNetCore.csproj"
COPY . .
WORKDIR "/src/MyNetCore"
RUN dotnet build "MyNetCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyNetCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MyNetCore.dll"]