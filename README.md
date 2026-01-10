# BookingSys - Microservices Booking System

A comprehensive booking and reservation management system built with .NET 9, following Clean Architecture, Domain-Driven Design (DDD), and Event-Driven Architecture patterns.

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              API Gateway (YARP)                              │
│                            http://localhost:5000                             │
│                    JWT Authentication + Reverse Proxy                        │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
          ┌───────────────────────────┼───────────────────────────┐
          │                           │                           │
          ▼                           ▼                           ▼
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│  Reservation    │       │  Availability   │       │    Payment      │
│    Service      │◄─────►│    Service      │       │    Service      │
│   :5001         │       │   :5002         │       │   :5003         │
└────────┬────────┘       └────────┬────────┘       └────────┬────────┘
         │                         │                         │
         └─────────────────────────┼─────────────────────────┘
                                   │
                          ┌────────▼────────┐
                          │    RabbitMQ     │
                          │   Event Bus     │
                          │   :5672/:15672  │
                          └────────┬────────┘
                                   │
    ┌──────────────────────────────┼──────────────────────────────┐
    │                              │                              │
    ▼                              ▼                              ▼
┌─────────┐                 ┌─────────────┐               ┌─────────────┐
│ Policy  │                 │Notification │               │  Reporting  │
│ Worker  │                 │   Worker    │               │   Service   │
└────┬────┘                 └──────┬──────┘               │   :5006     │
     │                             │                      └──────┬──────┘
     │                             │                             │
     │                             │                      ┌──────▼──────┐
     │                             │                      │   Audit     │
     │                             │                      │   Service   │
     │                             │                      │   :5007     │
     └─────────────────────────────┼──────────────────────┴─────────────┘
                                   │
                          ┌────────▼────────┐
                          │   PostgreSQL    │
                          │   :5433         │
                          └─────────────────┘
```

## 🚀 Quick Start

### Prerequisites
- Docker & Docker Compose
- .NET 9 SDK (for local development)
- PowerShell or Bash

### Start the System

```bash
cd C:\Users\Usuario\Documents\Proyects\BookingSys
docker compose up --build -d
```

### Verify All Services

```bash
docker compose ps
```

### Stop the System

```bash
docker compose down
```

## 📦 Microservices

### Core Services

| Service | Port | Description |
|---------|------|-------------|
| **API Gateway** | 5000 | Unified entry point with YARP routing and JWT authentication |
| **ReservationService** | 5001 | Core reservation management with Saga orchestration |
| **AvailabilityService** | 5002 | Time slot checking and locking |
| **PaymentService** | 5003 | Payment settlement processing |
| **ReportingService** | 5006 | CQRS read models for analytics and reports |
| **AuditService** | 5007 | Immutable event log for auditing |

### Background Workers

| Worker | Description |
|--------|-------------|
| **PolicyWorker** | Business rules enforcement (no-shows, cancellations, client blocks) |
| **NotificationWorker** | Simulated email/SMS notifications |

### Infrastructure

| Service | Port | Description |
|---------|------|-------------|
| **PostgreSQL** | 5433 | Primary database |
| **RabbitMQ** | 5672 / 15672 | Message broker (AMQP / Management UI) |

## 🔐 Authentication

### Register a New User

```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
    "email": "john@example.com",
    "password": "password123",
    "firstName": "John",
    "lastName": "Doe"
}
```

### Login

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
    "email": "john@example.com",
    "password": "password123"
}
```

**Response:**
```json
{
    "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "User",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-01-04T12:00:00Z"
}
```

### Get Current User

```http
GET http://localhost:5000/api/auth/me
Authorization: Bearer <token>
```

## 📋 API Endpoints

### Reservations

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/reservations` | Create a new reservation |
| GET | `/api/reservations` | Get all reservations |
| GET | `/api/reservations/{id}` | Get reservation by ID |

**Create Reservation:**
```http
POST http://localhost:5000/api/reservations
Content-Type: application/json

{
    "clientId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "start": "2026-01-05T10:00:00Z",
    "end": "2026-01-05T12:00:00Z"
}
```

### Payments

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/payments/settle` | Settle a payment for a reservation |

**Settle Payment:**
```http
POST http://localhost:5000/api/payments/settle
Content-Type: application/json

{
    "reservationId": "019b868d-2c5f-7403-8a09-f843d0ec07d6",
    "amount": 150.00
}
```

### Reports

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reports/occupancy?start=&end=` | Get occupancy report |
| GET | `/api/reports/daily-stats?date=` | Get daily statistics |
| GET | `/api/reports/cancellations?start=&end=` | Get cancellation report |

### Audit

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/audit` | Get all audit entries |
| GET | `/api/audit/entity/{id}` | Get audit entries for entity |

## 🔄 Saga Flow

The reservation process follows a choreography-based Saga pattern:

```
1. Client POST /api/reservations
         │
         ▼
2. ReservationService creates reservation (Pending)
   └── Publishes: ReservationRequested
         │
         ▼
3. AvailabilityService checks slot
   ├── Available → Publishes: AvailabilityLocked
   └── Unavailable → Publishes: AvailabilityRejected
         │
         ▼
4. ReservationService handles response
   ├── Locked → Status: Confirmed, Publishes: ReservationConfirmed
   └── Rejected → Status: Cancelled, Publishes: ReservationCancelled
         │
         ▼
5. Client POST /api/payments/settle
         │
         ▼
6. PaymentService processes payment
   └── Publishes: PaymentSettled
         │
         ▼
7. ReservationService completes saga
   └── Status: Completed, Publishes: ReservationCompleted
```

## 📊 Event Types

| Event | Publisher | Consumers |
|-------|-----------|-----------|
| `ReservationRequested` | ReservationService | AvailabilityService, ReportingService, AuditService |
| `AvailabilityLocked` | AvailabilityService | ReservationService |
| `AvailabilityRejected` | AvailabilityService | ReservationService |
| `ReservationConfirmed` | ReservationService | NotificationWorker, ReportingService, AuditService |
| `ReservationCancelled` | ReservationService | PolicyWorker, NotificationWorker, ReportingService, AuditService |
| `PaymentSettled` | PaymentService | ReservationService, ReportingService, AuditService |
| `ReservationCompleted` | ReservationService | NotificationWorker, ReportingService, AuditService |
| `NoShowReported` | External | PolicyWorker |
| `PenaltyApplied` | PolicyWorker | AuditService |
| `ClientBlocked` | PolicyWorker | NotificationWorker, AuditService |


## 🛠️ Development

### Project Structure

```
BookingSys/
├── docker-compose.yml
├── README.md
└── Services/
    ├── ApiGateway/
    │   └── Gateway.Api/
    │       ├── Controllers/
    │       ├── DTOs/
    │       ├── Entities/
    │       ├── Persistence/
    │       ├── Repositories/
    │       └── Services/
    ├── ReservationService/
    │   ├── Reservation.Api/
    │   ├── Reservation.Application/
    │   ├── Reservation.Domain/
    │   └── Reservation.Infrastructure/
    ├── AvailabilityService/
    │   ├── Availability.Api/
    │   ├── Availability.Application/
    │   ├── Availability.Domain/
    │   └── Availability.Infrastructure/
    ├── PaymentService/
    │   ├── Payment.Api/
    │   ├── Payment.Application/
    │   ├── Payment.Domain/
    │   └── Payment.Infrastructure/
    ├── PolicyService/
    │   ├── Policy.Worker/
    │   ├── Policy.Application/
    │   ├── Policy.Domain/
    │   └── Policy.Infrastructure/
    ├── NotificationService/
    │   ├── Notification.Worker/
    │   ├── Notification.Application/
    │   ├── Notification.Domain/
    │   └── Notification.Infrastructure/
    ├── ReportingService/
    │   ├── Reporting.Api/
    │   ├── Reporting.Application/
    │   ├── Reporting.Domain/
    │   └── Reporting.Infrastructure/
    ├── AuditService/
    │    ├── Audit.Api/
    │    ├── Audit.Application/
    │    ├── Audit.Domain/
    │    └── Audit.Infrastructure/
    └── Shared
        ├── Events/
        ├── Interfaces/
        └── Messaging/
```

### Run Locally

```bash
# Start infrastructure
docker compose up -d postgres rabbitmq

# Run services
cd Services/ReservationService/Reservation.Api
dotnet run

cd Services/AvailabilityService/Availability.Api
dotnet run

# ... etc
```

### Add Database Migrations

```bash
cd Services/ReservationService
dotnet ef migrations add MigrationName --project Reservation.Infrastructure --startup-project Reservation.Api
```

## 🧪 Testing

### Full Flow Test (PowerShell)

```powershell
# 1. Register user
$registerBody = @{
    email = "test@test.com"
    password = "test123"
    firstName = "Test"
    lastName = "User"
} | ConvertTo-Json

$user = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/register" -Method Post -Body $registerBody -ContentType "application/json"

# 2. Login
$loginBody = @{
    email = "test@test.com"
    password = "test123"
} | ConvertTo-Json

$auth = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $auth.token
$userId = $auth.userId

Write-Host "User ID: $userId"
Write-Host "Token: $token"

# 3. Create reservation
$reservationBody = @{
    clientId = $userId
    start = (Get-Date).AddDays(1).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    end = (Get-Date).AddDays(1).AddHours(2).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
} | ConvertTo-Json

$reservation = Invoke-RestMethod -Uri "http://localhost:5000/api/reservations" -Method Post -Body $reservationBody -ContentType "application/json" -Headers @{Authorization="Bearer $token"}
$reservationId = $reservation.id

Write-Host "Reservation ID: $reservationId"

# 4. Wait for saga
Start-Sleep -Seconds 3

# 5. Check status
$status = Invoke-RestMethod -Uri "http://localhost:5000/api/reservations/$reservationId" -Method Get
Write-Host "Status after availability check: $($status.status)"

# 6. Settle payment
$paymentBody = @{
    reservationId = $reservationId
    amount = 150.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/payments/settle" -Method Post -Body $paymentBody -ContentType "application/json"
Write-Host "Payment settled!"

# 7. Wait and verify completed
Start-Sleep -Seconds 3
$final = Invoke-RestMethod -Uri "http://localhost:5000/api/reservations/$reservationId" -Method Get
Write-Host "Final Status: $($final.status)"

# 8. Check reports
$today = (Get-Date).ToString("yyyy-MM-dd")
$reports = Invoke-RestMethod -Uri "http://localhost:5000/api/reports/daily-stats?date=$today" -Method Get
Write-Host "Daily Stats:"
$reports | ConvertTo-Json

# 9. Check audit
$audit = Invoke-RestMethod -Uri "http://localhost:5000/api/audit/entity/$reservationId" -Method Get
Write-Host "Audit Entries: $($audit.Count)"
```

## 📈 Monitoring

- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)
- **Health Checks**: `GET /health` on each service
- **Docker Logs**: `docker compose logs -f <service-name>`
- **PostgreSQL**: Connect via any PostgreSQL client to `localhost:5433`

## 🔒 Security

- JWT-based authentication with 24-hour expiration
- SHA256 password hashing
- Role-based authorization (User, Admin)
- CORS configured for development (allow all origins)
- Secure communication between services via Docker network

## 🎯 Key Features

- **Saga Pattern**: Choreography-based distributed transactions
- **Event Sourcing**: Complete audit trail of all events
- **CQRS**: Separate read models for reporting
- **Clean Architecture**: Separation of concerns across layers
- **Domain-Driven Design**: Rich domain models
- **Microservices**: Independently deployable services
- **Event-Driven**: Asynchronous communication via RabbitMQ
- **Idempotent Event Handlers**: Prevents duplicate processing
- **Policy Enforcement**: No-show tracking and client blocking
- **Real-time Notifications**: Event-driven notification system


---

**Built with ❤️ using .NET 9, Clean Architecture, and Event-Driven Design**
