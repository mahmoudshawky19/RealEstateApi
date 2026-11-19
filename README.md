# 🏠 RealEstate API

![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-blue)
![Unit Tests](https://img.shields.io/badge/Unit%20Tests-116%2B-success)
![License](https://img.shields.io/badge/License-MIT-blue)

A RESTful Web API for a Real Estate Marketplace built with **ASP.NET Core 10**, following **Clean Architecture** principles.

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Configuration](#️-configuration)
- [API Endpoints](#-api-endpoints)
- [Database](#️-database)
- [Testing](#-testing)
- [Authentication & Authorization](#-authentication--authorization)
- [Error Handling](#️-error-handling)
- [Contributing](#-contributing)
- [License](#-license)
- [Author](#-author)

---

## 🎯 Overview

**RealEstate API** is a backend system for a real estate marketplace that allows users to list, browse, search, and inquire about properties available for sale or rent.

The platform supports individual and company accounts, property management with an admin approval workflow, favorites, reviews, inquiries, notifications, and an administrative dashboard.

## ✨ Features

### 👤 Authentication & Account Management
- User registration and login using JWT Authentication.
- Role-based authorization (Admin, User).
- Support for Individual and Company accounts.
- Profile management.
- Change password.
- Forgot and reset password functionality.

### 🏘️ Property Management
- Create, update, and delete properties.
- Admin approval workflow (Pending → Approved / Rejected).
- Featured properties management.
- Advanced property search, filtering, and sorting.
- Related properties recommendations.
- Property status lifecycle:
  - Pending
  - Approved
  - Rejected
  - Sold
  - Rented

### 🖼️ Property Images
- Upload multiple images per property.
- Set a primary property image.
- Image validation (file size, type, and file signature).
- Local file storage.

### 📍 Location Management (Admin)
- Cities CRUD operations.
- Areas CRUD operations.
- Areas linked to their respective cities.

### 🗂️ Categories (Admin)
- Create, update, delete, and retrieve property categories.

### ❤️ Favorites
- Add and remove favorite properties.
- Check favorite status.
- Retrieve all favorite properties for the authenticated user.

### 💬 Inquiries
- Send inquiries about properties.
- Property owners can view and manage received inquiries.
- Update inquiry status (New, InProgress, Replied, Closed).

### ⭐ Reviews
- One review per user per property.
- Rating system (1–5 stars).
- Optional review comments.
- Calculate average property ratings.
- Update and delete reviews.

### 🔔 Notifications
Automatically generated notifications for:
- New property inquiries.
- Property approval or rejection.
- New property reviews.

Additional features:
- Mark notifications as read.
- Retrieve unread notifications.
- Get unread notification count.
- Delete notifications.

### 📊 Admin Dashboard
- Overview statistics (users, properties, inquiries, and reviews).
- Retrieve recent properties.
- Retrieve recent registered users.
- Retrieve recent inquiries.

---

## 🏗️ Architecture

The solution follows **Clean Architecture** principles with a clear separation of concerns.

```text
┌─────────────────────────────────────────────┐
│              Presentation (API)             │
│       Controllers, Middlewares, DI          │
├─────────────────────────────────────────────┤
│                Infrastructure               │
│       EF Core, Identity, Services            │
├─────────────────────────────────────────────┤
│                 Application                 │
│           DTOs, Service Interfaces           │
├─────────────────────────────────────────────┤
│                    Domain                   │
│          Entities, Enums, Business Rules     │
└─────────────────────────────────────────────┘
```

| Layer | Responsibility |
|---|---|
| Domain | Core entities, enums, and business rules. No external dependencies. |
| Application | DTOs, service interfaces, and application contracts. |
| Infrastructure | EF Core, database configurations, Identity, and service implementations. |
| RealEstate (API) | Controllers, middlewares, dependency injection, and application entry point. |

---

## 🛠️ Tech Stack

| Technology | Version / Purpose |
|---|---|
| .NET | 10.0 |
| ASP.NET Core Web API | 10.0 |
| Entity Framework Core | 10.0 |
| SQL Server | Database |
| ASP.NET Core Identity | 10.0 |
| JWT Bearer Authentication | 10.0 |
| Swagger / Swashbuckle | API Documentation |
| xUnit | Unit Testing |
| Moq | Mocking Dependencies |
| EF Core InMemory | In-Memory Database Testing |

---

## 📁 Project Structure

```text
RealEstate/
│
├── Domain/
│   ├── Entities/
│   │   ├── Property.cs
│   │   ├── Category.cs
│   │   ├── City.cs
│   │   ├── Area.cs
│   │   ├── Favorite.cs
│   │   ├── Inquiry.cs
│   │   ├── Review.cs
│   │   ├── Notification.cs
│   │   └── PropertyImage.cs
│   │
│   └── Enums/
│       ├── AccountType.cs
│       ├── ListingType.cs
│       ├── PropertyStatus.cs
│       └── InquiryStatus.cs
│
├── Application/
│   ├── DTOs/
│   └── Interfaces/
│
├── Infrastructure/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/
│   │
│   ├── Identity/
│   │   ├── ApplicationUser.cs
│   │   ├── RoleNames.cs
│   │   └── IdentitySeeder.cs
│   │
│   └── Services/
│
├── Tests/
│   └── Services/
│       ├── AuthServiceTests.cs
│       ├── CategoryServiceTests.cs
│       ├── CityServiceTests.cs
│       ├── DashboardServiceTests.cs
│       ├── FavoriteServiceTests.cs
│       ├── InquiryServiceTests.cs
│       ├── LocalImageStorageServiceTests.cs
│       ├── NotificationServiceTests.cs
│       ├── PropertyImageServiceTests.cs
│       └── PropertyServiceTests.cs
│
└── RealEstate/
    ├── Controllers/
    ├── Middlewares/
    │   ├── GlobalExceptionMiddleware.cs
    │   └── RequestLoggingMiddleware.cs
    │
    ├── Program.cs
    └── appsettings.json
```

---

## 🚀 Getting Started

### Prerequisites

Make sure you have installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server or SQL Server Express.
- Visual Studio 2022+, VS Code, or JetBrains Rider.
- Entity Framework Core CLI.

Install EF Core CLI if you haven't already:

```bash
dotnet tool install --global dotnet-ef
```

### Installation

**1. Clone the repository**

```bash
git clone https://github.com/mahmoudshawky19/RealEstate.git
```

**2. Navigate to the project directory**

```bash
cd RealEstate
```

**3. Configure the database connection**

Update the connection string in `RealEstate/appsettings.json` or use User Secrets.

```json
{
  "ConnectionStrings": {
    "RealEstateDatabase": "Data Source=YOUR_SERVER;Initial Catalog=RealEstateDb;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

**4. Apply database migrations**

```bash
dotnet ef database update --project Infrastructure --startup-project RealEstate
```

**5. Run the application**

```bash
dotnet run --project RealEstate
```

**6. Open Swagger UI**

```text
https://localhost:7258/swagger
```

The application is now ready to use.

---

## ⚙️ Configuration

### appsettings.json

The application requires the following configuration:

```json
{
  "ConnectionStrings": {
    "RealEstateDatabase": "your-connection-string"
  },
  "Jwt": {
    "Issuer": "RealEstateAPI",
    "Audience": "RealEstateClient",
    "SecretKey": "your-secret-key-at-least-32-characters"
  },
  "Admin": {
    "Email": "admin@example.com"
  }
}
```

> ⚠️ **Security Note:** Never commit real credentials, JWT secrets, or production connection strings to source control. Use .NET User Secrets or environment variables.

### Admin Seeding

On application startup, the Identity Seeder:

1. Creates the predefined roles (Admin, User).
2. Checks the configured admin email.
3. Assigns the Admin role to the registered user matching `Admin:Email`.

To create an admin account:

1. Set the admin email in configuration.
2. Register a user using that email.
3. Restart the application.
4. The seeder will automatically assign the Admin role.

---

## 🔌 API Endpoints

All endpoints are prefixed with `/api`.

**Authentication Legend:**
- 🌐 Public endpoint (No authentication required).
- 🔐 Authenticated endpoint.
- 👑 Admin only.

### 🔐 Authentication

| Method | Endpoint | Description | Access |
|---|---|---|---|
| POST | `/api/auth/register` | Register a new user | 🌐 |
| POST | `/api/auth/login` | Login and get JWT token | 🌐 |

### 👤 Account Management

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/account/profile` | Get current user profile | 🔐 |
| PUT | `/api/account/profile` | Update profile | 🔐 |
| PUT | `/api/account/change-password` | Change password | 🔐 |
| POST | `/api/account/forgot-password` | Request password reset | 🌐 |
| POST | `/api/account/reset-password` | Reset password using token | 🌐 |

### 🏘️ Properties

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/properties` | Get all approved properties | 🌐 |
| GET | `/api/properties/{id}` | Get property by ID | 🌐 |
| GET | `/api/properties/my-properties` | Get current user's properties | 🔐 |
| GET | `/api/properties/featured` | Get featured properties | 🌐 |
| GET | `/api/properties/pending` | Get pending properties | 👑 |
| GET | `/api/properties/search?searchTerm=` | Search properties | 🌐 |
| GET | `/api/properties/filter` | Filter properties | 🌐 |
| GET | `/api/properties/sort` | Sort properties | 🌐 |
| GET | `/api/properties/{id}/related` | Get related properties | 🌐 |
| POST | `/api/properties` | Create a property | 🔐 |
| PUT | `/api/properties/{id}` | Update a property | 🔐 |
| DELETE | `/api/properties/{id}` | Delete a property | 🔐 |
| PUT | `/api/properties/{id}/status` | Approve or reject a property | 👑 |
| PUT | `/api/properties/{id}/featured` | Mark property as featured | 👑 |
| DELETE | `/api/properties/{id}/featured` | Remove property from featured | 👑 |

### 🖼️ Property Images

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/propertyimages/{propertyId}` | Get property images | 🌐 |
| POST | `/api/propertyimages/{propertyId}/upload` | Upload property image | 🔐 |
| PUT | `/api/propertyimages/{imageId}/primary` | Set primary image | 🔐 |
| DELETE | `/api/propertyimages/{imageId}` | Delete property image | 🔐 |

### 🗂️ Categories

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/categories` | Get all categories | 🌐 |
| POST | `/api/categories` | Create category | 👑 |
| PUT | `/api/categories/{id}` | Update category | 👑 |
| DELETE | `/api/categories/{id}` | Delete category | 👑 |

### 📍 Cities

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/cities` | Get all cities | 🌐 |
| GET | `/api/cities/{id}` | Get city by ID | 🌐 |
| POST | `/api/cities` | Create city | 👑 |
| PUT | `/api/cities/{id}` | Update city | 👑 |
| DELETE | `/api/cities/{id}` | Delete city | 👑 |

### 🗺️ Areas

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/areas` | Get all areas | 🌐 |
| GET | `/api/areas/{id}` | Get area by ID | 🌐 |
| POST | `/api/areas` | Create area | 👑 |
| PUT | `/api/areas/{id}` | Update area | 👑 |
| DELETE | `/api/areas/{id}` | Delete area | 👑 |

### ❤️ Favorites

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/favorites` | Get user favorites | 🔐 |
| POST | `/api/favorites/{propertyId}` | Add property to favorites | 🔐 |
| DELETE | `/api/favorites/{propertyId}` | Remove property from favorites | 🔐 |
| GET | `/api/favorites/isFavorite/{propertyId}` | Check favorite status | 🔐 |

### 💬 Inquiries

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/inquiries` | Get sent inquiries | 🔐 |
| GET | `/api/inquiries/received` | Get received inquiries | 🔐 |
| GET | `/api/inquiries/{id}` | Get inquiry by ID | 🔐 |
| POST | `/api/inquiries` | Create inquiry | 🔐 |
| PUT | `/api/inquiries/{id}/status` | Update inquiry status | 🔐 |

### ⭐ Reviews

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/reviews/property/{propertyId}` | Get property reviews | 🔐 |
| GET | `/api/reviews/property/{propertyId}/average-rating` | Get average rating | 🔐 |
| POST | `/api/reviews` | Create review | 🔐 |
| PUT | `/api/reviews/{id}` | Update review | 🔐 |
| DELETE | `/api/reviews/{id}` | Delete review | 🔐 |

### 🔔 Notifications

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/notifications` | Get all notifications | 🔐 |
| GET | `/api/notifications/unread` | Get unread notifications | 🔐 |
| GET | `/api/notifications/unread-count` | Get unread count | 🔐 |
| PUT | `/api/notifications/{id}/read` | Mark notification as read | 🔐 |
| PUT | `/api/notifications/read-all` | Mark all notifications as read | 🔐 |
| DELETE | `/api/notifications/{id}` | Delete notification | 🔐 |

### 📊 Admin Dashboard

| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | `/api/dashboard/overview` | Get dashboard statistics | 👑 |
| GET | `/api/dashboard/recent-properties?count=5` | Get recent properties | 👑 |
| GET | `/api/dashboard/recent-users?count=5` | Get recent users | 👑 |
| GET | `/api/dashboard/recent-inquiries?count=5` | Get recent inquiries | 👑 |

---

## 🗄️ Database

### Entity Relationships

```text
City (1) ──────────── (N) Area
Area (1) ──────────── (N) Property
Category (1) ──────── (N) Property

Property (1) ──────── (N) PropertyImage

Property (1) ──────── (N) Favorite (N) ──────── (1) ApplicationUser

Property (1) ──────── (N) Inquiry (N) ───────── (1) ApplicationUser

Property (1) ──────── (N) Review (N) ────────── (1) ApplicationUser

ApplicationUser (1) ─ (N) Notification
```

### Key Constraints

- **Unique Review:** One review per user per property using a composite unique index.
- **Rating Validation:** Rating must be between 1 and 5 using a database check constraint.
- **Cascade Delete:** Configured cascade delete relationships for supported entities.
- **Restrict Delete:** Category and Area deletion is restricted when related properties exist.

### Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName --project Infrastructure --startup-project RealEstate
```

Apply migrations:

```bash
dotnet ef database update --project Infrastructure --startup-project RealEstate
```

---

## 🧪 Testing

The project includes **116+ Unit Tests** using xUnit, Moq, and EF Core InMemory to ensure service reliability and business logic correctness.

### Testing Tools

| Tool | Purpose |
|---|---|
| xUnit | Unit testing framework |
| Moq | Mocking dependencies |
| EF Core InMemory | In-memory database testing |

### Test Coverage

| Service | Tests |
|---|---:|
| AuthService | 6 |
| CategoryService | 8 |
| CityService | 8 |
| DashboardService | 10 |
| FavoriteService | 8 |
| InquiryService | 13 |
| LocalImageStorageService | 6 |
| NotificationService | 15 |
| PropertyImageService | 14 |
| PropertyService | 28 |
| **Total** | **116+** |

### Testing Approach

- Arrange-Act-Assert (AAA) pattern.
- Isolated database instance for each test.
- Mocking external dependencies using Moq.
- Testing success and failure scenarios.
- Testing authorization and validation rules.
- Verifying service interactions and side effects.

### Running Tests

Run all tests:

```bash
dotnet test
```

Run tests with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Run a specific test class:

```bash
dotnet test --filter "FullyQualifiedName~AuthServiceTests"
```

Generate code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🔐 Authentication & Authorization

The API uses **JWT Bearer Authentication** to secure protected endpoints.

Include the JWT token in the HTTP Authorization header:

```http
Authorization: Bearer YOUR_JWT_TOKEN
```

### Roles & Permissions

| Role | Permissions |
|---|---|
| User | Manage own properties, favorites, inquiries, reviews, and notifications. |
| Admin | Manage categories, cities, and areas; approve/reject properties; manage featured properties; access the admin dashboard. |

### Getting an Admin Account

1. Configure the admin email in `Admin:Email`.
2. Register a user with the same email.
3. Restart the application.
4. The Identity Seeder automatically assigns the Admin role.

---

## 🛡️ Error Handling

The API uses **ProblemDetails (RFC 7807)** through a Global Exception Middleware to provide consistent and structured error responses.

Example response:

```json
{
  "status": 404,
  "title": "Resource Not Found",
  "detail": "The requested resource was not found.",
  "instance": "/api/properties/999",
  "traceId": "0HN7..."
}
```

### Exception Mapping

| Exception | HTTP Status |
|---|---|
| KeyNotFoundException | 404 Not Found |
| UnauthorizedAccessException | 403 Forbidden |
| ArgumentException | 400 Bad Request |
| Any other unhandled exception | 500 Internal Server Error |

---

## 🤝 Contributing

Contributions are welcome!

1. Fork the repository.
2. Create a feature branch:

```bash
git checkout -b feature/amazing-feature
```

3. Commit your changes:

```bash
git commit -m "Add amazing feature"
```

4. Push to your branch:

```bash
git push origin feature/amazing-feature
```

5. Open a Pull Request.

---

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Mahmoud Shawky**

- GitHub: [@mahmoudshawky19](https://github.com/mahmoudshawky19)

---

<p align="center">
  Made with ❤️ using ASP.NET Core 10
</p>
