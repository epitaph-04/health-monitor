FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-preview.5 AS setup
WORKDIR /src
RUN apt update -y && apt install python3 -y
RUN dotnet workload install wasm-tools

FROM setup AS restore
COPY . .
RUN dotnet restore

FROM restore AS publish
RUN dotnet publish "health-monitor/health-monitor.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0-preview-noble-chiseled AS final
EXPOSE 8080
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./health-monitor"]