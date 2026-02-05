FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ConsumerApi.slnx", "src/ConsumerApi/"]
COPY ["ConsumerApi.csproj", "src/ConsumerApi/"]

RUN dotnet restore "src/ConsumerApi/ConsumerApi.slnx"
COPY . "src/ConsumerApi/"

RUN dotnet build "src/ConsumerApi/ConsumerApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ConsumerApi/ConsumerApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ConsumerApi.dll"]
