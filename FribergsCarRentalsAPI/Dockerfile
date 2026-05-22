FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FribergsCarRentalsAPI/FribergCarRentalsAPI.csproj", "FribergsCarRentalsAPI/"]
COPY ["FribergCarRentalsABClient/FribergCarRentalsABClient.csproj", "FribergCarRentalsABClient/"]
RUN dotnet restore "FribergsCarRentalsAPI/FribergCarRentalsAPI.csproj"
COPY . .
WORKDIR "/src/FribergsCarRentalsAPI"
RUN dotnet build "FribergCarRentalsAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FribergCarRentalsAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FribergCarRentalsAPI.dll"]
