FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
EXPOSE 5168 5068 8000 5432 29092 5076 5074
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "Gateway.dll"]