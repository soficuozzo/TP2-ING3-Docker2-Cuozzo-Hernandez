

## Elección de la aplicación y tecnología utilizada
Se implementó una **Web API en .NET 7 (C#)** porque:
- Es el framework que vimos en clase y tiene soporte oficial.
- Permite exponer endpoints REST de forma rápida.
- La integración con contenedores Docker está muy bien soportada por Microsoft.
- Es suficientemente simple para una demo y suficientemente potente para producción.

Endpoints principales:
- `/WeatherForecast` → endpoint de ejemplo con datos aleatorios.
- `/db/ping` → prueba de conexión a la base de datos.
- `/db/init` → crea la tabla `weather` en la base seleccionada.
- `/db/seed` → inserta datos de prueba.
- `/db/all` → devuelve todos los registros insertados.

---

## Elección de imagen base

Se utilizó un **Dockerfile multi-stage**:
- **Build stage:** `mcr.microsoft.com/dotnet/sdk:7.0`  
  → compila y publica la aplicación en Release.
- **Runtime stage:** `mcr.microsoft.com/dotnet/aspnet:7.0`  
  → corre la aplicación en un contenedor más liviano.

Justificación:
- La imagen final es mucho más chica y segura (no contiene el SDK completo).
- El build queda cacheado, acelerando recompilaciones.
- Es el enfoque recomendado por Microsoft para contenerizar apps .NET.

---

## Elección de base de datos

Se eligió **MySQL 8.0** por:
- Es una imagen oficial muy utilizada en producción.
- Fácil integración con .NET a través del paquete **MySqlConnector**.
- Amplio soporte y documentación.
- Ya incluye inicialización automática de la base a través de la variable `MYSQL_DATABASE`.

---

## Estructura del Dockerfile

Pasos clave:
1. Copia el `.csproj` y ejecuta `dotnet restore` (descarga dependencias).
2. Copia el resto del código y publica en `/app/publish`.
3. Imagen final expone el puerto 80 y ejecuta con `dotnet SimpleWebAPI.dll`.

Elegí esta estructura porque:
- Sigue buenas prácticas (multi-stage).
- Evita hardcodear secretos en la imagen.
- Configura conexión a DB y entorno a través de **variables de entorno** (se inyectan desde `docker-compose.yml`).

---

## Configuración de QA y PROD (variables de entorno)

La **misma imagen de app (`soficuozzo/simplewebapi:v1.0`)** se ejecuta en dos servicios distintos:

- `app-qa`  
  - `ASPNETCORE_ENVIRONMENT=QA`  
  - `ConnectionStrings__Default=Server=db-qa;Port=3306;Database=simpledb_qa;User ID=root;Password=SofiCuoZZo#2025!;AllowPublicKeyRetrieval=True;SslMode=None;`

- `app-prod`  
  - `ASPNETCORE_ENVIRONMENT=Production`  
  - `ConnectionStrings__Default=Server=db-prod;Port=3306;Database=simpledb_prod;User ID=root;Password=SofiCuoZZo#2025!;AllowPublicKeyRetrieval=True;SslMode=None;`

Esto garantiza que:
- Los dos entornos corran exactamente el mismo binario.
- Los datos de QA y PROD estén **separados** gracias a bases, redes y volúmenes distintos.

---

## Estrategia de persistencia de datos

Cada MySQL tiene su volumen:
- `mysqldata_qa:/var/lib/mysql`
- `mysqldata_prod:/var/lib/mysql`

Beneficios:
- Los datos persisten aunque los contenedores se reinicien.
- Los entornos son totalmente independientes: lo que se guarda en QA no aparece en PROD y viceversa.

---

## Estrategia de versionado y publicación

Imágenes publicadas en Docker Hub bajo el usuario **`soficuozzo`**:
- `soficuozzo/simplewebapi:latest` → versión de desarrollo.
- `soficuozzo/simplewebapi:v1.0` → versión estable entregada para la práctica.

Esta convención permite distinguir entre versiones en desarrollo y releases listas para entregar.

---

## Para evidenciar el funcionamiento 

- **Aplicación corriendo en ambos entornos:**
  - `docker compose ps` muestra:
    - `mysql-simplewebapi-qa` (healthy)
    - `app-qa` (Up en 8080)
    - `mysql-simplewebapi-prod` (healthy)
    - `app-prod` (Up en 8081)

- **Conexión exitosa a la base:**
  - `curl.exe http://localhost:8080/db/ping` devuelve 200 OK en QA.
  - `curl.exe http://localhost:8081/db/ping` devuelve 200 OK en PROD.

- **Datos independientes:**
  1. En QA: `POST /db/seed` y luego `GET /db/all` → lista con 5 filas.
  2. En PROD: `GET /db/all` → vacío hasta que se ejecute `/db/seed`.
  3. Se prueba que lo cargado en QA **no aparece** en PROD y viceversa.

- **Persistencia:**
  - Insertar datos.
  - Ejecutar `docker compose restart`.
  - Los datos siguen presentes en cada entorno.

---

## Problemas y soluciones

- **Error 500 en `/db/ping` al inicio:**  
  → La app arrancaba antes de que MySQL estuviera listo.  
  *Solución:* se agregó `healthcheck` en `db-qa` y `db-prod` + `depends_on: condition: service_healthy`.

- **“Cannot connect to MySQL” con MySQL 8:**  
  → Error de autenticación por defecto.  
  *Solución:* se agregó `command: --default-authentication-plugin=mysql_native_password` y parámetros `AllowPublicKeyRetrieval=True;SslMode=None;` en la cadena de conexión.

- **Datos mezclados entre QA y PROD:**  
  → Al inicio se compartía el mismo volumen.  
  *Solución:* se crearon volúmenes separados (`mysqldata_qa` y `mysqldata_prod`) y redes distintas (`qa_net` y `prod_net`).

---
