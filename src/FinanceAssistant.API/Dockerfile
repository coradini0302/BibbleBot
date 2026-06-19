FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/FinanceAssistant.API/FinanceAssistant.API.csproj", "src/FinanceAssistant.API/"]
COPY ["src/FinanceAssistant.Application/FinanceAssistant.Application.csproj", "src/FinanceAssistant.Application/"]
COPY ["src/FinanceAssistant.Domain/FinanceAssistant.Domain.csproj", "src/FinanceAssistant.Domain/"]
COPY ["src/FinanceAssistant.Infrastructure/FinanceAssistant.Infrastructure.csproj", "src/FinanceAssistant.Infrastructure/"]

RUN dotnet restore "src/FinanceAssistant.API/FinanceAssistant.API.csproj"

COPY . .

WORKDIR "/src/src/FinanceAssistant.API"
RUN dotnet build "FinanceAssistant.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FinanceAssistant.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceAssistant.API.dll"]
