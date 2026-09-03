BancoSol - API de Gestión Financiera Personal.
Wilson Luque.

API REST desarrollada en **.NET 8** bajo los principios de **Clean Architecture**, **SOLID** y buenas practicas. 
Permite el registro de ingresos monetarios con 2 monedas (BOB y USD), consulta de historial, obtencion de tipo de cambio en tiempo real mediante **HexaRate API** y generacion de balances.


**Despliegue en Produccion (Cloud)**

- **Documentacion Interactiva Swagger UI**: https://bancosol-finanzas-api-production.up.railway.app/swagger
- **Plataforma de Hosting**: Railway (Contenedor Docker Linux + Base de datos PostgreSQL)


**Arquitectura del Sistema (Clean Architecture)**

El proyecto sigue una arquitectura en 4 capas, bases de datos y bibliotecas externas:


                        BS.FINANZAS.API
                    (Presentacion / Web API)                     
                                 │
                                 ▼
                      BS.FINANZAS.Infrastructure   
                     (Dapper, Postgres, HTTP) 
                                 │
                                 ▼
                       BS.FINANZAS.Application     
                     (Casos de Uso, DTOs, Logic)                  
                                 │
                                 ▼
                        BS.FINANZAS.Domain       
                    (Entidades, Enums, Contratos)
                  


**1. BS.FINANZAS.Domain (Capa de Dominio)**
- **Responsabilidad**: Contiene las entidades esenciales y contratos fundamentales del negocio sin dependencias externas.
- **Componentes**:
  - Entities/Ingreso.cs: Entidad central, monto, descripcion, fecha, fuente y moneda.
  - Entities/Moneda.cs: Enumeracion (BOB, USD).
  - Interfaces/IIngresoRepository.cs: Contrato para operaciones.

**2. BS.FINANZAS.Application (Capa de Aplicacion)**
- **Responsabilidad**: Orquesta los casos de uso, implementa la logica de conversion.
- **Componentes**:
  - DTOs/Requests,Responses: Objetos de transferencia de datos con validaciones: 
        (CrearIngresoRequestDto, IngresoResponseDto, BalanceReportRequestDto, BalanceReportResponseDto).

  - Interfaces/IIngresoService.cs: Contrato de los servicios.
  - Interfaces/IHexaRateService.cs: Contrato para la consulta de tipo de cambio.
  - Services/IngresoService.cs: Logica de negocio (conversion matematica, validaciones y balance).

**3. BS.FINANZAS.Infrastructure (Capa de Infraestructura)**
- **Responsabilidad**: acceso a recursos externos (Base de Datos PostgreSQL, llamadas HTTP externas).
- **Componentes**:
  - Repositories/DapperIngresoRepository.cs: Acceso de alto rendimiento a PostgreSQL utilizando **Dapper** y consultas SQL parametrizadas.
  - Repositories/InMemoryIngresoRepository.cs: Implementacion en memoria (ConcurrentDictionary) para pruebas unitarias.
  - Services/HexaRateService.cs: Consumo de la API externa de tipo de cambio con tolerancia a fallos.
  - Models/IngresoDbModel.cs: Modelo de mapeo hacia la tabla de PostgreSQL.

**4. BS.FINANZAS.API (Capa de Presentacin / API REST)**
- **Responsabilidad**: Expone los endpoints HTTP, gestiona CORS, inyeccion de dependencias y genera la documentacion OpenAPI.
- **Componentes**:
  - Controllers/IngresosController.cs: Controlador RESTful con codigos de estado HTTP (200, 201, 400, 404, 500).
  - Program.cs: Configuracion de servicios, middleware, inicializacion de BD y Swagger UI.

**5. BS.FINANZAS.Tests (Capa de Pruebas automatizadas)**
- **Responsabilidad**: Validaciones automatizadas con **xUnit** y **Moq** (Caso de Uso 7).
- Valida: Rechazo de monedas no soportadas, calculo de balance en Bolivianos y calculo de balance en Dolares.

**Patrones de Diseño y Principios SOLID**

- Desacopla la logica de negocio de la implementacion de persistencia (PostgreSQL vs. In-Memory).
- **Dependency Injection**: control total en constructores para facilitar el testing y la modularidad.
- **Tipo HttpClient (IHttpClientFactory)**: manejo de sockets HTTP para el consumo de HexaRate API.
- **(DTO)**: aisla la estructura interna de la base de datos de interfaces expuestos al exterior.
- **Principio de Responsabilidad única**: cada clase posee una unica razon para cambiar.
- **Tolerancia a Fallos Fallback**: si el servicio externo no esta disponible, la aplicacion recurre a la tasa por defecto configurada y no interrumpir el flujo.

**endpoints de la API REST**

POST /api/ingresos  CU1 ->          Registra un nuevo ingreso en BOB o USD
GET  /api/ingresos  CU2 ->          Consulta el historial de ingresos
GET  /api/ingresos/{id} CU3 ->      Obtiene los detalles de un ingreso especifico
GET  /api/ingresos/tipo-cambio CU4 -> Consulta el tipo de cambio actual USD/BOB
GET  /api/ingresos/balance CU5 ->   Reporte de balance por rango de fechas
GET  /swagger  CU6 ->               Documentacion interactiva Swagger UI

**Instrucciones para ejecutar localmente**

**Prerrequisitos**
- .NET 8 SDK
- Docker Desktop o desde cli.

**Ejecucion con Docker (PostgreSQL) y .NET CLI

1. **Clonar el repositorio**:
2. **Iniciar Bds PostgreSQL en Docker**:
   docker run --name postgres-bancosol -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=bancosol_db -p 5432:5432 -d postgres

3. **Ejecutar la API REST**:   
   dotnet run --project BS.FINANZAS.API/BS.FINANZAS.API.csproj   

4. **Acceder a Swagger UI**:
   - Abre tu navegador en: https://localhost:port/swagger


**ejecucion de Pruebas Unitarias (Caso de Uso 7)**

En power shell:
dotnet test

**Resultado esperado**:
Serie de pruebas: BS.FINANZAS.API.Tests.dll
Correctas! - Con error: 0, Superado: 4, Omitido: 0, Total: 4

**Variables de Configuracion (appsettings.json)**


{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=BancoSol_Finanzas;Username=postgres;Password=postgres"
  },
  "HexaRate": {
    "ApiUrl": "https://hexarate.paikama.co/api/rates/USD/BOB/latest",
    "DefaultRate": 12.155
  }
}

En caso de no encontrar 'DefaultConnection', el sistema activa el repositorio en memoria para continuar con la operacion.