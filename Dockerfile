# Club it API container. Build from the repository root: docker build -t clubit-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the dependency layer is cached between builds.
COPY global.json Directory.Build.props ./
COPY src/ClubIt.Domain/ClubIt.Domain.csproj src/ClubIt.Domain/
COPY src/ClubIt.Contracts/ClubIt.Contracts.csproj src/ClubIt.Contracts/
COPY src/ClubIt.Application/ClubIt.Application.csproj src/ClubIt.Application/
COPY src/ClubIt.Infrastructure/ClubIt.Infrastructure.csproj src/ClubIt.Infrastructure/
COPY src/ClubIt.Api/ClubIt.Api.csproj src/ClubIt.Api/
RUN dotnet restore src/ClubIt.Api/ClubIt.Api.csproj

COPY src/ClubIt.Domain/ src/ClubIt.Domain/
COPY src/ClubIt.Contracts/ src/ClubIt.Contracts/
COPY src/ClubIt.Application/ src/ClubIt.Application/
COPY src/ClubIt.Infrastructure/ src/ClubIt.Infrastructure/
COPY src/ClubIt.Api/ src/ClubIt.Api/
RUN dotnet publish src/ClubIt.Api/ClubIt.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ClubIt.Api.dll"]
