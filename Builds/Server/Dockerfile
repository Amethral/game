# Image Ubuntu l�g�re
FROM ubuntu:22.04

# Installation des d�pendances Unity
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    ca-certificates \
    libglu1-mesa \
    libxcursor1 \
    libxrandr2 \
    libxinerama1 \
    libxi6 && \
    rm -rf /var/lib/apt/lists/*

# Cr�e le r�pertoire de travail
WORKDIR /app

# Copie tous les fichiers du build
COPY . /app

# Permissions d'ex�cution
RUN chmod +x /app/server.x86_64

# Expose le port FishNet (UDP + TCP)
EXPOSE 7770/udp
EXPOSE 7770/tcp

# Lance le serveur en mode headless
# -batchmode : mode non-interactif
# -nographics : pas de rendu graphique
# -logFile : affiche les logs dans stdout
CMD ["./server.x86_64", "-batchmode", "-nographics", "-logFile", "/dev/stdout", "-port", "7770", "-ip", "0.0.0.0"]

