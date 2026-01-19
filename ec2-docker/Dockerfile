FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EC2.slnx", "./"]
COPY ["src/EC2.API/EC2.API.csproj", "src/EC2.API/"]

RUN dotnet restore "EC2.slnx"
COPY . .
RUN dotnet build "src/EC2.API/EC2.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/EC2.API/EC2.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EC2.API.dll"]
