# Day 5: CI/CD, Containerization, Observability, and Capstone Demonstration

This day marks the culmination of the 5-day software engineering lifecycle for the **LIRS Taxpayer Mini-Portal**. We transition our application from a local development prototype into an automated, containerized, enterprise-ready system. Today covers Continuous Integration and Continuous Delivery (CI/CD) with GitHub Actions, containerization using Docker and Docker Compose, production health checks and observability, completing the final backlog items (return approval and revenue reporting), and delivering the executive capstone demonstration to stakeholders.

```
Developer Push (Git) ──► GitHub Actions Pipeline (CI)
                              │
                              ├──► 1. Lint & Format Check
                              ├──► 2. Backend .NET Build & Unit Tests (xUnit)
                              ├──► 3. Frontend React Build (Vite)
                              └──► 4. Docker Container Image Packaging
                                        │
                                        ▼ (CD / Deployment)
 ┌────────────────────────────────────────────────────────────────────────┐
 │ Production Runtime Environment (Docker Compose / Cloud Host)           │
 │                                                                        │
 │  ┌───────────────────────┐      ┌───────────────────────────────────┐  │
 │  │ Nginx Reverse Proxy   │      │ ASP.NET Core API (.NET 10/8)      │  │
 │  │ React Portal (:5173)  │─────►│ Health Check (:5123/health)       │  │
 │  └───────────────────────┘      │ REST Endpoints & Auth             │  │
 │                                 └─────────────────┬─────────────────┘  │
 │                                                   │                    │
 │                                                   ▼                    │
 │                                     ┌───────────────────────────────┐  │
 │                                     │ Microsoft SQL Server          │  │
 │                                     │ Database: LirsPortal          │  │
 │                                     └───────────────────────────────┘  │
 └────────────────────────────────────────────────────────────────────────┘
```

---

## 1. The DevOps Philosophy and Continuous Integration / Continuous Delivery

**DevOps** is the union of people, process, and products to enable continuous delivery of value to end users, dissolving the traditional wall between software development and IT operations.
*Example:* Instead of developers emailing compiled zip files to the infrastructure team to manually paste onto servers at midnight, automated pipelines build, test, and release verified code continuously.

**Continuous Integration (CI)** is the automated practice of merging developer code into a shared repository multiple times daily, followed by automated compilation and test execution.
*Example:* Whenever a developer opens a Pull Request on GitHub, an automated runner immediately compiles the C# API, runs all 25 xUnit tests, and validates that no existing feature was broken before the code can be merged into `main`.

**Continuous Delivery (CD)** is the automated release practice where code changes that pass CI are automatically packaged and staged so they can be deployed to production safely at any time with minimal human intervention.
*Example:* Merging an approved PR automatically triggers a build that packages the application into a Docker container image and deploys it to a staging or production server.

**The "Works on My Machine" Syndrome** is the operational failure where code runs properly on an individual developer's computer but crashes in production due to configuration discrepancies, missing runtime dependencies, or environment differences.
*Example:* A developer tests with SQL Server Developer Edition on Windows 11 with specific regional date formats, but the production Linux server fails because localized decimal commas break monetary calculations.

**Infrastructure as Code (IaC)** is the management and provisioning of compute, network, and storage infrastructure through declarative definition files rather than manual physical configuration.
*Example:* Defining our SQL database, backend API, and React frontend containers inside a single version-controlled `docker-compose.yml` file.

**Deployment Risk Reduction** principles dictate that small, frequent, automated deployments reduce operational risk compared to massive, quarterly releases.
*Example:* Deploying small, peer-reviewed 50-line pull requests daily means that if a bug appears, pinpointing and rolling back the exact defect takes minutes rather than days.

---

## 2. Automated Pipelines with GitHub Actions

**GitHub Actions** is a continuous integration and continuous delivery platform that automates build, test, and deployment pipelines through event-driven workflows directly within GitHub repositories.

**A Workflow** is a configurable automated process defined in a YAML file located inside the `.github/workflows/` directory of a repository.
*Example:* A file named `.github/workflows/ci.yml` that executes whenever code is pushed to the `main` branch or when a Pull Request is opened.

**An Event** is a specific repository trigger that starts a workflow execution.
*Example:* `push`, `pull_request`, or `schedule`.

**A Job** is a series of sequential steps that execute on the same runner virtual machine.
*Example:* A `backend-build-test` job and a `frontend-build` job that run concurrently to reduce pipeline runtime.

**A Step** is an individual task within a job, which can either run a shell command or execute a reusable community action.
*Example:* Running `actions/setup-dotnet@v4` to configure the .NET SDK, followed by `dotnet test` to run test suites.

**A Runner** is a virtual machine hosted by GitHub (or self-hosted on enterprise premises) that executes pipeline jobs.
*Example:* An `ubuntu-latest` or `windows-latest` virtual machine spun up on demand by GitHub to execute test suites in isolated environments.

### The Anatomy of an Enterprise GitHub Actions Workflow

```yaml
name: LIRS Portal CI Pipeline

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  backend-test:
    name: Build & Test Backend API
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Source Code
        uses: actions/checkout@v4

      - name: Setup .NET SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore Dependencies
        run: dotnet restore backend/LirsPortal.Api

      - name: Compile API Project
        run: dotnet build backend/LirsPortal.Api --no-restore --configuration Release

      - name: Run Automated Tests
        run: dotnet test backend/LirsPortal.Tests --no-build --verbosity normal

  frontend-build:
    name: Build React Vite Portal
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Source Code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'

      - name: Install Frontend Dependencies
        working-directory: frontend/portal
        run: npm ci

      - name: Build Production Assets
        working-directory: frontend/portal
        run: npm run build
```

---

## 3. Containerization with Docker

**Containerization** is an OS-level virtualization method for deploying and running distributed applications without launching an entire guest operating system for each application.
*Example:* Packaging the ASP.NET Core API with all its required .NET runtime binaries into an isolated container that runs identically on Windows, Linux, macOS, or Azure.

**Virtual Machine (VM) vs. Container:**
* **Virtual Machine:** Virtualizes physical hardware. Each VM includes a full guest operating system (gigabytes in size), has a multi-minute boot time, and incurs substantial memory overhead.
* **Container:** Virtualizes the operating system kernel. Containers share the host OS kernel, start in sub-second time, use megabytes of storage, and run as isolated processes.

**A Dockerfile** is a text script containing sequential instructions that Docker uses to assemble a container image.
*Example:* Instructions specifying base runtime, working directory, file copies, compilation steps, and the entrypoint command.

**A Container Image** is an immutable, read-only template with runtime instructions used to create active Docker containers.
*Example:* `lirs-portal-api:latest`, which contains compiled C# binaries, configuration files, and the minimal runtime.

**A Container** is a runnable, isolated instance of an image.
*Example:* A running process executing `dotnet Lirsportal.Api.dll` listening on port 5123 inside its own network namespace.

**Multi-Stage Builds** are an optimization pattern in Docker where intermediate stages compile code with heavy SDKs, and only the final compiled binaries are copied into a lightweight, secure production base image.
*Example:* Using the 800 MB .NET SDK image to build and compile the API, then copying the resulting 15 MB published binaries into a 100 MB minimal ASP.NET runtime image.

### Production Multi-Stage Dockerfile for ASP.NET Core Web API

```dockerfile
# Stage 1: Build & Compile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/LirsPortal.Api/*.csproj ./backend/LirsPortal.Api/
RUN dotnet restore backend/LirsPortal.Api/*.csproj

COPY backend/LirsPortal.Api/ ./backend/LirsPortal.Api/
WORKDIR /src/backend/LirsPortal.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app
EXPOSE 5123

ENV ASPNETCORE_URLS=http://+:5123
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lirsportal.Api.dll"]
```

### Production Dockerfile for React Frontend with Nginx

```dockerfile
# Stage 1: Build React Production Bundle
FROM node:20-alpine AS build
WORKDIR /app

COPY frontend/portal/package*.json ./
RUN npm ci

COPY frontend/portal/ ./
RUN npm run build

# Stage 2: High-Performance Static Web Server (Nginx)
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY frontend/portal/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## 4. Multi-Container Orchestration with Docker Compose

**Docker Compose** is a declarative tool for defining and running multi-container Docker applications using a single YAML configuration file.
*Example:* Spinning up the SQL Server database, the ASP.NET Core API, and the React frontend simultaneously with one command: `docker compose up -d`.

**Service Dependency & Health Checks:** In a multi-tier system, the API cannot start until the database is ready to accept connections. Docker Compose provides health checks and `depends_on` conditions to enforce startup ordering.

```yaml
version: '3.8'

services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: lirs-db
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=LirsSecure2026!
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

  api:
    build:
      context: .
      dockerfile: backend/LirsPortal.Api/Dockerfile
    container_name: lirs-api
    ports:
      - "5123:5123"
    environment:
      - ConnectionStrings__LirsDb=Server=db,1433;Database=LirsPortal;User Id=sa;Password=LirsSecure2026!;TrustServerCertificate=True
    depends_on:
      - db

  portal:
    build:
      context: .
      dockerfile: frontend/portal/Dockerfile
    container_name: lirs-frontend
    ports:
      - "5173:80"
    depends_on:
      - api

volumes:
  sqldata:
```

---

## 5. Observability, Health Checks, and Production Monitoring

**Observability** is the degree to which the internal state of a software system can be inferred solely by examining its external telemetry outputs.

**The Three Pillars of Observability:**
1. **Logs:** Immutable, timestamped textual records of discrete events (e.g. `[INFO] Payment 5 submitted for Return 2 by Taxpayer 101`).
2. **Metrics:** Aggregable numeric measurements recorded over intervals of time (e.g. requests per second, CPU utilization, 95th percentile response latency).
3. **Traces:** End-to-end representations of a single request journey as it traverses multiple network boundaries and services.

**Structured Logging** is the practice of outputting logs in serialized JSON format with machine-readable key-value properties rather than unstructured plain text strings.
*Example:* Emitting `{"timestamp":"2026-10-07T14:30:00Z","level":"Information","event":"PaymentRecorded","taxpayerId":101,"amount":50000.00,"receipt":"RCT-000005"}` allows automated monitoring tools (like Datadog or Elasticsearch) to query and alert on specific transaction amounts.

**Health Checks** are standardized HTTP endpoints that probe internal system dependencies and report whether an instance is operational.
* **Liveness Probe:** Indicates whether the application process is alive or crashed.
* **Readiness Probe:** Indicates whether the application is fully initialized and capable of handling incoming client traffic (e.g. database connections established, cache warmed up).

```csharp
// Adding built-in ASP.NET Core Health Checks in Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("LirsDb")!,
        name: "sqlserver",
        timeout: TimeSpan.FromSeconds(3));

app.MapHealthChecks("/health");
```

---

## 6. Regulatory Compliance and the Audit Trail (NDPR)

**The Nigeria Data Protection Act (NDPA / NDPR)** governs the lawful collection, processing, and retention of personal and financial information of citizens.
*Example:* Taxpayer records containing Taxpayer Identification Numbers (TIN), phone numbers, and financial declarations must be protected against unauthorized disclosure and tampering.

**Non-Repudiation** is the assurance that an individual cannot deny the authenticity of an action or transaction they performed.
*Example:* When Officer Bisi approves a tax return or changes a taxpayer's status, the system must write an immutable record into `ComplianceLogs` capturing the officer's verified identity, the exact action taken, and the server timestamp.

**Principles of Audit Logging in Government Financial Systems:**
1. **Immutability:** Audit tables must be append-only. No `UPDATE` or `DELETE` permissions should exist on the `ComplianceLogs` table.
2. **Attribution:** Every critical record modification must link directly to the authenticated user ID and role extracted from the cryptographic JWT.
3. **Completeness:** Both successful operations and rejected attempts (such as unauthorized role escalations) must be logged.

---

## 7. Completing the Day 5 Product Backlog

Today completes the two remaining high-value User Stories from the Product Backlog:

### US-06: Officer Review and Approval of Submitted Tax Returns
* **Actor:** LIRS Revenue Officer (`Role = "Officer"`)
* **Business Need:** Before a tax assessment becomes legally binding and final, an officer must verify the declared income against submitted documents and approve or reject the submission.
* **Backend Endpoint:** `PATCH /api/returns/{id}/status`
* **Security Rule:** Taxpayers cannot approve their own returns (`[Authorize(Roles = "Officer")]`).
* **Compliance:** The system writes directly to `ComplianceLogs` upon approval.

### US-07: Officer Statewide Collection Reporting
* **Actor:** LIRS Revenue Officer / Executive Management
* **Business Need:** Agency leadership requires real-time analytics comparing total revenue collected across local governments and states to evaluate compliance campaigns.
* **Backend Endpoint:** `GET /api/reports/collections-by-state`
* **SQL Aggregation:**
  ```sql
  SELECT 
      t.State,
      COUNT(p.PaymentId) AS TotalTransactions,
      SUM(p.Amount) AS TotalCollected
  FROM Payments p
  INNER JOIN TaxReturns r ON p.ReturnId = r.ReturnId
  INNER JOIN Taxpayers t ON r.TaxpayerId = t.TaxpayerId
  GROUP BY t.State
  ORDER BY TotalCollected DESC;
  ```
* **Frontend Screen:** Renders an executive summary card and comparative data table on the Officer Dashboard.

---

## 8. Capstone Demonstration & Executive Handover

The final milestone of Day 5 is the **Capstone Demonstration**. This is not a casual code walkthrough; it is a formal technical and executive showcase proving that the LIRS Taxpayer Mini-Portal satisfies all functional requirements, security standards, and operational guidelines established on Day 1.

### The 4 Pillars of the Executive Demonstration:

| Pillar | Target Audience | Key Elements Demonstrated |
|---|---|---|
| **1. The Citizen Experience** | Public Taxpayers / Product Owners | Fast responsive UI, balance lookup in under 2 seconds, payment submission, instant receipt generation, mobile layout. |
| **2. The Revenue Officer Experience** | Tax Auditors / Operations Leads | 10-digit TIN directory search, on-demand balance retrieval, return verification queue, one-click approval, statewide revenue reports. |
| **3. Information Security & Compliance** | Chief Information Security Officer (CISO) | Cryptographic JWT tokens, role guards, zero plain-text passwords (PBKDF2), SQL injection immunity, immutable audit trail. |
| **4. DevOps & Operational Reliability** | Infrastructure & Cloud Engineers | Automated GitHub Actions CI pipeline, Docker containerization, zero-downtime health probes, structured logging. |

---

## 9. Words Used Today, in One Line Each

| Word | Plain Meaning |
|---|---|
| **DevOps** | The union of software development and IT operations to deliver quality software continuously. |
| **CI (Continuous Integration)** | Automatically compiling code and running automated tests on every code push. |
| **CD (Continuous Delivery)** | Automatically packaging and staging verified code for immediate production release. |
| **Pipeline** | The automated sequence of steps (build, test, package, deploy) triggered by repository events. |
| **GitHub Actions** | GitHub's built-in platform for executing automated workflows in virtual machine runners. |
| **Container** | A lightweight, standalone, executable package containing an application and everything needed to run it. |
| **Docker** | The industry-standard platform for building, sharing, and running containerized applications. |
| **Image** | An immutable snapshot containing the operating system layer, runtime, and compiled application code. |
| **Multi-stage build** | A Dockerfile technique that separates build SDK tools from the final minimal production image. |
| **Docker Compose** | A configuration tool used to define and run multi-container applications with a single command. |
| **Observability** | The ability to measure and understand the internal state of a system through external logs, metrics, and traces. |
| **Health Check** | An automated HTTP endpoint (`/health`) used by load balancers and orchestrators to verify service status. |
| **Structured Logging** | Writing log events as key-value JSON records rather than unsearchable plain text. |
| **Non-Repudiation** | Ensuring a user cannot deny an action they performed by capturing verified identity and audit logs. |
| **NDPR** | The Nigeria Data Protection Regulation governing lawful and secure handling of citizen data. |
| **Capstone** | The culminating final project demonstration proving that all training objectives have been mastered. |
