🌐 SimpleWebAPI + Docker + MySQL

Este proyecto es un **Trabajo Práctico 02 – Introducción a Docker (2025)**.  
Se containerizó una aplicación **ASP.NET Core Web API** junto con bases de datos **MySQL**, levantando entornos separados de **QA** y **PROD**.


📋 Requisitos previos

- [Docker](https://www.docker.com/get-started) (Docker Engine + Compose)
- [Postman](https://www.postman.com/) o `curl` para probar los endpoints
- Cuenta en [Docker Hub](https://hub.docker.com/) (la imagen está publicada en `soficuozzo/simplewebapi`)

---

## 🏗️ Estructura del proyecto

- **/SimpleWebAPI** → Código fuente en .NET 7 (controladores y modelos).
- **Dockerfile** → Multi-stage build para compilar y ejecutar la app.
- **docker-compose.yml** → Orquesta los contenedores (QA y PROD).
- **decisiones.md** → Documento con las decisiones técnicas del proyecto.
- **README.md** → Este archivo ✨.

---

## 🚀 Levantar el proyecto

Clonar el repositorio y ubicarse en la raíz del proyecto:

```bash
git clone https://github.com/soficuozzo/tp2-docker.git
cd tp2-docker
1. Construir la imagen (si querés hacerlo local)
bash
Copiar código
docker build -t soficuozzo/simplewebapi:v1.1 .
2. Levantar los servicios
bash
Copiar código
docker compose up -d
Esto crea los siguientes contenedores:

QA

mysql-simplewebapi-qa (MySQL con datos persistentes en mysqldata_qa)

simplewebapi-qa (API en http://localhost:8080)

PROD

mysql-simplewebapi-prod (MySQL con datos persistentes en mysqldata_prod)

simplewebapi-prod (API en http://localhost:8081)

🔗 Endpoints disponibles
WeatherForecast (ejemplo incluido en .NET)
QA → http://localhost:8080/WeatherForecast

PROD → http://localhost:8081/WeatherForecast

Endpoints de base de datos (ejemplo con Dapper/MySQL)
GET /db/ping → Prueba conexión a la base

POST /db/init → Crea la tabla si no existe

POST /db/seed → Inserta datos de prueba

GET /db/all → Devuelve todos los registros

Ejemplo con curl:

bash
Copiar código
# QA
curl http://localhost:8080/db/ping
curl -X POST http://localhost:8080/db/init
curl -X POST http://localhost:8080/db/seed
curl http://localhost:8080/db/all

# PROD
curl http://localhost:8081/db/ping
📦 Datos persistentes
Los datos de QA viven en mysqldata_qa (volumen de Docker).

Los datos de PROD viven en mysqldata_prod.
👉 Lo que insertes en QA no se verá en PROD y viceversa.

🔑 Variables de entorno
En docker-compose.yml se definen:

yaml
Copiar código
MYSQL_ROOT_PASSWORD: SofiCuoZZo#2025!
MYSQL_DATABASE: simpledb_qa / simpledb_prod
ASPNETCORE_ENVIRONMENT: QA / Production
ConnectionStrings__Default: "...cadena de conexión a MySQL..."
🐳 Imagen en Docker Hub
La imagen está publicada en:
👉 soficuozzo/simplewebapi

Tags disponibles:

v1.0 → Versión inicial estable

v1.1 → Última versión con integración MySQL

latest → Última build publicada

Ejemplo para correr directo desde Docker Hub:

bash
Copiar código
docker run -p 8080:80 soficuozzo/simplewebapi:v1.1
✅ Evidencia de funcionamiento
QA y PROD levantados en simultáneo (docker compose ps → ambos en Up).

curl o Postman muestran respuestas distintas en cada entorno.

Los datos persisten entre reinicios (docker compose restart).

