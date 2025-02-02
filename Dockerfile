# Étape 1 : Build de l'application .NET
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier le fichier .env dans le conteneur
COPY .env /app/.env

# Charger les variables d'environnement
ENV DOTNET_ENVIRONMENT=Production
ENV ACCESS_TOKEN=${ACCESS_TOKEN}
ENV INSTAGRAM_ACCOUNT_ID=${INSTAGRAM_ACCOUNT_ID}

COPY ["JokeXPBot.csproj", "./"]
RUN dotnet restore "JokeXPBot.csproj"

COPY . .
RUN dotnet publish "JokeXPBot.csproj" -c Release -o /app/publish

# Étape 2 : Utilisation d'une image .NET 9.0 avec les dépendances requises pour SkiaSharp
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Installation des dépendances natives pour SkiaSharp
RUN apt-get update && apt-get install -y \
    libfontconfig1 \
    libfreetype6 \
    libjpeg62-turbo \
    libpng16-16 \
    libexpat1 \
    libharfbuzz0b \
    libx11-6 \
    libc6 \
    libstdc++6 \
    libgif7 \
    libwebp7 \
    libicu-dev \
    --no-install-recommends && \
    rm -rf /var/lib/apt/lists/*

# Création du dossier de fonts et copie des fonts système
RUN mkdir -p /usr/share/fonts/truetype && \
    cp -r /usr/share/fonts/truetype /usr/local/share/fonts

COPY --from=build /app/publish .

COPY assets/ /app/assets/

RUN mkdir -p /app/wwwroot && chmod -R 777 /app/wwwroot

# Définir les variables d'environnement pour SkiaSharp
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "JokeXPBot.dll"]