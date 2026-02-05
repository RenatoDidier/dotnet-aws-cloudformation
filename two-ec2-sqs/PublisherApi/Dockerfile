FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PublisherApi.slnx", "src/PublisherApi/"]
COPY ["PublisherApi.csproj", "src/PublisherApi/"]

RUN dotnet restore "src/PublisherApi/PublisherApi.slnx"
COPY . "src/PublisherApi/"

RUN dotnet build "src/PublisherApi/PublisherApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PublisherApi/PublisherApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PublisherApi.dll"]
