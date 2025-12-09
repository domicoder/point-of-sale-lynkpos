# 🚀 Guía de Deploy en Railway con Azure SQL Database

Esta guía te ayudará a asegurar que las migraciones de Entity Framework se apliquen correctamente en Railway.

## 📋 Requisitos Previos

1. Proyecto desplegado en Railway
2. Azure SQL Database configurada
3. Variable de entorno `ConnectionStrings__DefaultConnection` configurada en Railway

## 🔧 Configuración en Railway

### 1. Variables de Entorno Necesarias

En el panel de Railway, configura las siguientes variables de entorno:

```bash
# Cadena de conexión a Azure SQL Database
ConnectionStrings__DefaultConnection=Server=tcp:TU_SERVIDOR.database.windows.net,1433;Initial Catalog=TU_BASE_DATOS;Persist Security Info=False;User ID=TU_USUARIO;Password=TU_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;

# Para aplicar migraciones automáticamente (opcional, por defecto se aplican en producción)
AUTO_MIGRATE=true

# Ambiente
ASPNETCORE_ENVIRONMENT=Production
```

### 2. Formato de Cadena de Conexión para Azure SQL

La cadena de conexión debe tener este formato:

```
Server=tcp:[servidor].database.windows.net,1433;
Initial Catalog=[nombre_base_datos];
Persist Security Info=False;
User ID=[usuario]@[servidor];
Password=[contraseña];
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

**Ejemplo:**

```
Server=tcp:lynkpos-sql.database.windows.net,1433;Initial Catalog=puntoventa_db;Persist Security Info=False;User ID=admin@lynkpos-sql;Password=MiPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## 🔄 Cómo Funciona la Aplicación Automática de Migraciones

### Comportamiento Automático

La aplicación aplicará migraciones automáticamente cuando:

1. **En Producción/Staging**: Siempre se aplican migraciones al iniciar
2. **Con `AUTO_MIGRATE=true`**: Se aplican migraciones independientemente del ambiente
3. **En Desarrollo**: NO se aplican automáticamente (debes hacerlo manualmente)

### Logs de Migración

Cuando la aplicación inicia, verás logs como estos:

```
info: Data.AppDbContext[0]
      Se encontraron 2 migraciones pendientes: 20251207_nueva_tabla, 20251208_actualizar_campo
info: Data.AppDbContext[0]
      Aplicando migraciones automáticamente...
info: Data.AppDbContext[0]
      Migraciones aplicadas exitosamente. Total aplicadas: 2
info: Data.AppDbContext[0]
      Conexión a la base de datos verificada exitosamente.
```

## 📝 Proceso de Deploy

### Paso 1: Crear Nueva Migración Localmente

```bash
# Desde la raíz del proyecto
cd Data
dotnet ef migrations add NombreDeTuMigracion --project ../Data/Data.csproj --startup-project ../API/API.csproj
```

### Paso 2: Verificar la Migración

```bash
# Ver el SQL que se generará
dotnet ef migrations script --project ../Data/Data.csproj --startup-project ../API/API.csproj
```

### Paso 3: Commit y Push

```bash
git add Data/Migrations/
git commit -m "feat: agregar migración [nombre]"
git push origin main
```

### Paso 4: Railway Aplicará Automáticamente

Cuando Railway despliegue tu código:

1. ✅ Detectará las nuevas migraciones
2. ✅ Las aplicará automáticamente al iniciar
3. ✅ Verificará la conexión a la base de datos
4. ✅ Si hay errores, la aplicación NO iniciará (fail-fast)

## 🛠️ Aplicar Migraciones Manualmente (Opcional)

Si prefieres aplicar migraciones manualmente antes del deploy:

### Opción 1: Desde Railway CLI

```bash
# Instalar Railway CLI
npm i -g @railway/cli

# Conectar a tu proyecto
railway login
railway link

# Aplicar migraciones
railway run dotnet ef database update --project Data/Data.csproj --startup-project API/API.csproj
```

### Opción 2: Script de Migración Separado

Puedes crear un servicio separado en Railway que solo ejecute migraciones:

```bash
# En Railway, crear un nuevo servicio con este comando:
dotnet ef database update --project Data/Data.csproj --startup-project API/API.csproj
```

## ⚠️ Troubleshooting

### Error: "Cannot open database"

**Causa**: La cadena de conexión no está configurada correctamente.

**Solución**:

1. Verifica que la variable `ConnectionStrings__DefaultConnection` esté configurada en Railway
2. Asegúrate de que el formato de la cadena sea correcto
3. Verifica que el firewall de Azure SQL permita conexiones desde Railway

### Error: "Migration already applied"

**Causa**: La migración ya fue aplicada previamente.

**Solución**: Esto es normal. La aplicación detectará que no hay migraciones pendientes y continuará.

### Error: "Timeout connecting to database"

**Causa**: El firewall de Azure SQL no permite conexiones desde Railway.

**Solución**:

1. Ve a Azure Portal → SQL Server → Firewall
2. Agrega la IP de Railway (puede cambiar)
3. O mejor: Habilita "Allow Azure services and resources to access this server"

### Verificar Estado de Migraciones

Puedes crear un endpoint temporal para verificar:

```csharp
[HttpGet("db/migrations")]
public IActionResult GetMigrations([FromServices] AppDbContext db)
{
    var applied = db.Database.GetAppliedMigrations();
    var pending = db.Database.GetPendingMigrations();

    return Ok(new {
        Applied = applied,
        Pending = pending,
        CanConnect = db.Database.CanConnect()
    });
}
```

## 🔒 Seguridad

### Variables de Entorno Sensibles

-   ✅ **NUNCA** commitees la cadena de conexión en el código
-   ✅ Usa variables de entorno en Railway
-   ✅ Usa Azure Key Vault para producción (opcional)

### Firewall de Azure SQL

1. Ve a Azure Portal
2. SQL Server → Security → Networking
3. Habilita "Allow Azure services and resources to access this server"
4. O agrega las IPs de Railway manualmente

## 📊 Monitoreo

### Ver Logs de Migración en Railway

1. Ve al dashboard de Railway
2. Selecciona tu servicio
3. Ve a la pestaña "Logs"
4. Busca mensajes que contengan "Migraciones" o "Database"

### Alertas

Configura alertas en Railway para:

-   Errores de conexión a la base de datos
-   Fallos en la aplicación de migraciones
-   Timeouts de conexión

## ✅ Checklist Pre-Deploy

Antes de hacer deploy, verifica:

-   [ ] Migraciones creadas y probadas localmente
-   [ ] Cadena de conexión configurada en Railway
-   [ ] Firewall de Azure SQL configurado
-   [ ] Variable `AUTO_MIGRATE` configurada (opcional)
-   [ ] Logs de Railway verificados después del deploy
-   [ ] Base de datos actualizada correctamente

## 🎯 Resumen

✅ **Las migraciones se aplican automáticamente** al iniciar la aplicación en Railway
✅ **No necesitas scripts adicionales** si configuras las variables de entorno correctamente
✅ **La aplicación fallará rápido** si hay problemas con las migraciones (mejor que iniciar con errores)
✅ **Los logs te dirán exactamente** qué migraciones se aplicaron
