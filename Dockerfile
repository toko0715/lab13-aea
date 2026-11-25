FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /src
COPY . .
RUN dotnet publish "$PROJECT/$PROJECT.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG PROJECT
ENV SERVICE_PROJECT=$PROJECT
ENV ASPNETCORE_HTTP_PORTS=8080
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["sh", "-c", "exec dotnet \"$SERVICE_PROJECT.dll\""]
