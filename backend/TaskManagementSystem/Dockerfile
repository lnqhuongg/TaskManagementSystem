# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file .csproj và restore dependencies
COPY ["TaskManagementSystem.csproj", "./"]
RUN dotnet restore "TaskManagementSystem.csproj"

# Copy toàn bộ mã nguồn và publish
COPY . .
RUN dotnet publish "TaskManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose cổng 8080 (cổng mặc định của .NET 9)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TaskManagementSystem.dll"]
