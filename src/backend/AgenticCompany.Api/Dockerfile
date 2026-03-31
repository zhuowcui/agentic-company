FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["AgenticCompany.Api/AgenticCompany.Api.csproj", "AgenticCompany.Api/"]
COPY ["AgenticCompany.Core/AgenticCompany.Core.csproj", "AgenticCompany.Core/"]
COPY ["AgenticCompany.Infrastructure/AgenticCompany.Infrastructure.csproj", "AgenticCompany.Infrastructure/"]
RUN dotnet restore "AgenticCompany.Api/AgenticCompany.Api.csproj"
COPY . .
RUN dotnet publish "AgenticCompany.Api/AgenticCompany.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AgenticCompany.Api.dll"]
