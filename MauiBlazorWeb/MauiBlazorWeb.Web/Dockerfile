# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MauiBlazorWeb/MauiBlazorWeb.Web/MauiBlazorWeb.Web.csproj", "MauiBlazorWeb.Web/"]
COPY ["MauiBlazorWeb/MauiBlazorWeb.Web.Client/MauiBlazorWeb.Web.Client.csproj", "MauiBlazorWeb.Web.Client/"]
COPY ["MauiBlazorWeb/MauiBlazorWeb.Shared/MauiBlazorWeb.Shared.csproj", "MauiBlazorWeb.Shared/"]

# Restore dependencies
RUN dotnet restore "MauiBlazorWeb.Web/MauiBlazorWeb.Web.csproj"

# Copy the rest of the code
COPY MauiBlazorWeb/ .

# Build the application
WORKDIR "/src/MauiBlazorWeb.Web"
RUN dotnet build "MauiBlazorWeb.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MauiBlazorWeb.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MauiBlazorWeb.Web.dll"]
