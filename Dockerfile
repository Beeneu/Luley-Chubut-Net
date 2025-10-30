# Adjust DOTNET_OS_VERSION as desired
ARG DOTNET_SDK_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS build

WORKDIR /src

# Copy csproj and restore dependencies
COPY Luley-Integracion-Net.csproj ./
RUN dotnet restore Luley-Integracion-Net.csproj

# Copy everything else
COPY . ./

# Build and publish to /app/publish
RUN dotnet publish Luley-Integracion-Net.csproj -c Release -o /app/publish

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_SDK_VERSION}

WORKDIR /app

# Install unixODBC and dependencies
RUN apt-get update && apt-get install -y \
    unixodbc \
    unixodbc-dev \
    wget \
    git \
    git-lfs \
    && rm -rf /var/lib/apt/lists/*

# Debug: List files in build context to verify hanaclient*.tar.gz exists
RUN echo "📦 Files in build context:" && ls -la /src

# Copy HANA client (managed by Git LFS locally)
COPY hanaclient*.tar.gz /tmp/hanaclient.tar.gz

# Debug: Verify file was copied and renamed correctly
RUN echo "📦 Files in /tmp:" && ls -lh /tmp/ && \
    if [ ! -f /tmp/hanaclient.tar.gz ]; then echo "❌ hanaclient.tar.gz not found!"; exit 1; fi

    # Copy HANA client (managed by Git LFS locally)
COPY hanaclient*.tar.gz /tmp/hanaclient.tar.gz

# Debug: Verify file was copied
RUN echo "📦 Files in /tmp:" && ls -lh /tmp/

# Extract and install HANA client
RUN cd /tmp && \
    tar -xzvf hanaclient.tar.gz && \
    ls -la && \
    cd client && \
    ./hdbinst -a client --path=/usr/sap/hdbclient && \
    cd / && \
    rm -rf /tmp/hanaclient.tar.gz /tmp/client /tmp/client


# Configure ODBC
RUN echo "[HDBODBC]" >> /etc/odbcinst.ini && \
    echo "Driver=/usr/sap/hdbclient/libodbcHDB.so" >> /etc/odbcinst.ini

ARG INTERNAL_PORT=5555
ENV ASPNETCORE_URLS=http://+:${INTERNAL_PORT}
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE ${INTERNAL_PORT}

# Copy published files
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Luley-Integracion-Net.dll"]