FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /app
COPY * ./
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /app
COPY --from=build  /app/out ./
ENV NYBUS_ENVIRONMENT=Production
ENTRYPOINT [ "dotnet", "NetCoreRabbitMq.dll" ]