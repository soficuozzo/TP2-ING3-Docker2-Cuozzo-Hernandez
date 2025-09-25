# --- Build & publish ---
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copiamos solo el csproj para cachear el restore
COPY SimpleWebAPI/SimpleWebAPI.csproj SimpleWebAPI/
RUN dotnet restore SimpleWebAPI/SimpleWebAPI.csproj

# Ahora sí copiamos el resto del código
COPY SimpleWebAPI/ SimpleWebAPI/

# Compilamos y publicamos
WORKDIR /src/SimpleWebAPI
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "SimpleWebAPI.dll"]
