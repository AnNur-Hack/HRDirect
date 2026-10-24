 HRDirectAPI – Employee Self-Service API

 Overview

HRDirectAPI is an Employee Self-Service backend API built with ASP.NET Core Web API. It is designed to support employee-related processes, training requests, learning management, and other HR operations through structured API endpoints.

The project follows a layered architecture to separate domain entities, data access, business logic, and HTTP endpoints.

 Technologies Used

* C#
* ASP.NET Core Web API
* .NET
* Microsoft SQL Server
* Entity Framework Core
* JWT Authentication
* Swagger / OpenAPI
* FluentValidation 

 Core Modules

 1. Authentication and Authorization

Supports authentication and role-based access control for authorized employees and administrators.

 2. Training Management

Supports employee training requests and approval workflows involving managers, HR, and Finance. It also includes withdrawal, completion evidence, and reviewer reassignment functionality.

 3. Learning Management System (LMS)

The LMS module is being developed to support:

* Course creation and publishing.
* Lesson and assessment question management.
* Course assignments with due dates.
* Employee lesson completion tracking.
* Assessment scoring and attempt limits.
* HR resets for exhausted assessment attempts.
* Assessment history and assignment tracking.

 Project Structure

HRDirectAPI/
├── Controllers/
├── Data/
├── Domain/
│   ├── Entities/
│   └── Enums/
├── DTOs/
├── Repositories/
│   ├── Implementations/
│   └── Interfaces/
├── Services/
│   ├── Implementations/
│   └── Interfaces/
├── appsettings.json
└── Program.cs


The project structure may evolve as additional modules are implemented.

 Getting Started

 Prerequisites

* .NET SDK compatible with the project.
* Microsoft SQL Server.
* An IDE such as Visual Studio or JetBrains Rider.

 Setup

1. Clone the repository.
2. Open the solution in your preferred IDE.
3. Configure the SQL Server connection string in `appsettings.json`.
4. Restore the project dependencies.
5. Apply the Entity Framework Core migrations to create or update the database.
6. Run the application.

Run the Application

From the project directory, run:

```bash
dotnet restore
dotnet build
dotnet run
```

When the application starts, use the Swagger interface to explore and test the available endpoints, if Swagger is enabled in the current environment.

 Configuration and Security

* Store database credentials and JWT secrets securely.
* Do not commit production credentials or sensitive configuration values.
* Configure authentication and authorization according to the application's access-control requirements.
* Validate incoming requests before processing them.

 Development Status

The project is under active development. Authentication, training management, and database configuration have been worked on. LMS entities, relationships, and initial request/response DTOs are being prepared for implementation.

Further work includes completing the LMS service layer, controllers, validation, and the remaining requirements in the capstone specification.

 Author

Munirudeen Akanbi

Employee Self-Service Capstone Project
