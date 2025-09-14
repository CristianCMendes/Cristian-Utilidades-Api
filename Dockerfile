FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
ENV DOTNET_ENVIRONMENT=Production

WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src


COPY ["Utilidades.Api/Utilidades.Api.csproj", "Utilidades.Api/"]
COPY ["./packages", "./packages"]
COPY ["nuget.config", ""]

RUN dotnet restore "Utilidades.Api/Utilidades.Api.csproj"
COPY . .
WORKDIR "/src/Utilidades.Api"
RUN dotnet build "./Utilidades.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build 

FROM build AS publish
ARG BUILD_CONFIGURATION=Release


RUN dotnet publish "./Utilidades.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app

USER $APP_UID

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utilidades.Api.dll"]


FROM final AS finalwithcert
WORKDIR /app

USER $APP_UID

USER root
RUN apt-get update && apt-get install -y --no-install-recommends libcap2-bin

COPY ["volumes/https/*", "/https/"]
# Garantir leitura dos certificados pelo usuário não-root e permitir bind <1024
RUN chown -R $APP_UID:$APP_UID /https && \
    setcap 'cap_net_bind_service=+ep' /usr/share/dotnet/dotnet && \
    apt-get purge -y --auto-remove libcap2-bin && \
    rm -rf /var/lib/apt/lists/*

COPY ["volumes/https/*", "/https/"]

ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/korsa.dev-server.pem
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/https/korsa.dev-server.key
ENV ASPNETCORE_URLS=https://+:443;http://+:80
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Utilidades.Api.dll"]

