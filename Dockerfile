FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowOps.Web/FlowOps.Web.csproj", "FlowOps.Web/"]
COPY ["FlowOps.Core/FlowOps.Core.csproj", "FlowOps.Core/"]
COPY ["FlowOps.Infrastructure/FlowOps.Infrastructure.csproj", "FlowOps.Infrastructure/"]
RUN dotnet restore "FlowOps.Web/FlowOps.Web.csproj"
COPY . .
RUN dotnet publish "FlowOps.Web/FlowOps.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "FlowOps.Web.dll"]
