#!/bin/bash

# Script para aplicar migraciones de Entity Framework
# Uso: ./scripts/apply-migrations.sh [ambiente]
# Ejemplo: ./scripts/apply-migrations.sh Production

set -e

ENVIRONMENT=${1:-Development}
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "🚀 Aplicando migraciones para ambiente: $ENVIRONMENT"
echo "📁 Directorio del proyecto: $PROJECT_ROOT"

cd "$PROJECT_ROOT"

# Verificar que dotnet ef está instalado
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: dotnet no está instalado"
    exit 1
fi

# Verificar que existe el proyecto Data
if [ ! -f "Data/Data.csproj" ]; then
    echo "❌ Error: No se encontró Data/Data.csproj"
    exit 1
fi

# Verificar que existe el proyecto API
if [ ! -f "API/API.csproj" ]; then
    echo "❌ Error: No se encontró API/API.csproj"
    exit 1
fi

echo "📦 Verificando migraciones pendientes..."

# Listar migraciones pendientes
PENDING_MIGRATIONS=$(dotnet ef migrations list --project Data/Data.csproj --startup-project API/API.csproj --no-build 2>/dev/null | grep "Pending" || true)

if [ -z "$PENDING_MIGRATIONS" ]; then
    echo "✅ No hay migraciones pendientes. La base de datos está actualizada."
    exit 0
fi

echo "📋 Migraciones pendientes encontradas:"
echo "$PENDING_MIGRATIONS"
echo ""

# Preguntar confirmación (solo en desarrollo)
if [ "$ENVIRONMENT" != "Production" ]; then
    read -p "¿Deseas aplicar estas migraciones? (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ Operación cancelada"
        exit 1
    fi
fi

echo "🔄 Aplicando migraciones..."

# Aplicar migraciones
dotnet ef database update --project Data/Data.csproj --startup-project API/API.csproj

if [ $? -eq 0 ]; then
    echo "✅ Migraciones aplicadas exitosamente!"
else
    echo "❌ Error al aplicar migraciones"
    exit 1
fi
