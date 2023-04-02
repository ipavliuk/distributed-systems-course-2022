#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ReplicatedLog.Master/ReplicatedLog.Master.csproj", "ReplicatedLog.Master/"]
RUN dotnet restore "ReplicatedLog.Master/ReplicatedLog.Master.csproj"
COPY . .
WORKDIR "/src/ReplicatedLog.Master"
RUN dotnet build "ReplicatedLog.Master.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReplicatedLog.Master.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReplicatedLog.Master.dll"]