# API Gateway

La API Gateway es el punto central de entrada para las solicitudes hacia los microservicios. Proporciona una capa de abstracción que maneja la autenticación, autorización y el enrutamiento de solicitudes hacia los microservicios correspondientes.

---

## Requisitos Previos

1. **Instalar .NET 8**
   - Asegúrate de tener el SDK de .NET instalado.
   - Verifica con:
     ```bash
     dotnet --version
     ```

2. **Archivo `.env`**
   - Crea un archivo `.env` en la raíz del proyecto con las siguientes variables:
     ```env
     # Servicio de autenticación
     JWT_AUTHORITY=http://localhost:5092
     AUTHSERVICE_BASEADDRESS=http://localhost:5092

     # Servicios de microservicios
     CARRERASSERVICE_BASEADDRESS=http://localhost:5190
     SUBJECTSERVICE_BASEADDRESS=http://localhost:5191
     ```

3. **Configurar los Microservicios**
   - Asegúrate de que los microservicios (`AuthService`, `CarrerasService`, `SubjectsService`) estén configurados correctamente y puedan ejecutarse.

---

## Pasos para Levantar la API Gateway

1. Asegúrate de estar en el directorio del proyecto de la API Gateway.

2. Restaura las dependencias del proyecto:
   ```bash
   dotnet restore
   ```

3. Ejecuta la aplicación en modo desarrollo:
   ```bash
   dotnet run
   ```

4. Verifica que la API Gateway esté corriendo en: `http://localhost:5036`

---

## Endpoints Disponibles

### **Autenticación (AuthService)**
- `POST /auth/register`: Registra un nuevo usuario.
- `POST /auth/login`: Inicia sesión y obtiene un token JWT.

### **Tokens**
- `POST /token/validate-token`: Valida si un token es válido.
- `POST /token/revoke-token`: Revoca un token JWT.

### **Carreras (CarrerasService)**
- `GET /carrera`: Obtiene información de una carrera específica.


### **Subjects (SubjectsService)**
- `GET /subject`: Obtiene la lista de subjects.
- `GET /prerequisito`: Obtiene la lista de prerrequisitos.
- `GET /postrequisito`: Obtiene la lista de postrequisitos.

---

## Problemas Comunes

1. **Error de Conexión a los Microservicios**:
   - Verifica que los microservicios estén en ejecución y sus puertos estén configurados correctamente.

2. **Variables de Entorno Incorrectas**:
   - Asegúrate de que el archivo `.env` contenga los valores correctos.

3. **Token Inválido o Faltante**:
   - Verifica que el encabezado `Authorization` contenga un token JWT válido.

---

© 2024 - Arquitectura de Sistemas

