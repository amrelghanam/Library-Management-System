# Library Management System

A RESTful API built with **ASP.NET Core 8** and **Entity Framework Core** for managing library books, members, and borrow transactions. The system supports role-based access control, activity logging, and full book catalog management.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core (SQL Server) |
| Auth | ASP.NET Core Identity (cookie-based) |
| API Docs | Swagger / OpenAPI |
| Database | SQL Server (LocalDB for development) |

---

## Project Structure

```
Library Management System/
├── Controllers/
│   ├── AuthControllerController.cs   # Register, Login, Logout
│   ├── BooksController.cs            # Full CRUD + search + status filter
│   ├── BorrowController.cs           # Borrow and return transactions
│   └── MembersController.cs          # Member CRUD
├── Domain/                           # Entity models
│   ├── Book.cs
│   ├── Author.cs
│   ├── Publisher.cs
│   ├── Category.cs
│   ├── Member.cs
│   ├── BorrowTransaction.cs
│   ├── ActivityLog.cs
│   ├── ApplicationUser.cs
│   ├── BookAuthor.cs                 # Join table
│   ├── BookCategory.cs               # Join table
│   └── BookStutas.cs                 # Enum: In / Out
├── Applications/
│   ├── Dtos/                         # Request DTOs
│   └── Interfaces/
│       └── IActivityLogService.cs
├── Infrastructure/
│   ├── Data/
│   │   ├── LibraryDbContext.cs
│   │   └── DbSeeder.cs               # Seeds roles + sample data
│   └── Services/
│       └── ActivityLogService.cs
├── Migrations/                       # EF Core migrations
├── Program.cs                        # App bootstrap & DI config
└── appsettings.json
```

---

## Design Choices

### 1. Cookie-Based Authentication (ASP.NET Core Identity)

The system uses ASP.NET Core Identity with **cookie-based authentication** rather than JWT tokens. This was chosen for simplicity in a traditional server-side context — Identity handles session management, password hashing, and role management out of the box. The cookie redirect behavior is overridden to return HTTP 401/403 status codes instead of HTML redirects, making the API behave correctly for JSON-consuming clients.

```csharp
options.Events.OnRedirectToLogin = context => {
    context.Response.StatusCode = 401;
    return Task.CompletedTask;
};
```

### 2. Role-Based Authorization (Three Roles)

Access is controlled through three predefined roles seeded at startup:

| Role | Permissions |
|---|---|
| **Administrator** | Full access — can delete books, manage all resources |
| **Librarian** | Can create/update books and members, issue/return books |
| **Staff** | Can issue/return books and search the catalog |

Roles are seeded automatically via `DbSeeder.SeedRolesAsync()` on application start, so no manual setup is needed.

### 3. Many-to-Many Relationships via Explicit Join Tables

Books can have multiple authors and belong to multiple categories. Rather than relying on EF Core's implicit many-to-many (which hides the join table), the design uses **explicit join entities** (`BookAuthor`, `BookCategory`) with composite primary keys configured in `OnModelCreating`. This keeps the schema transparent and makes it straightforward to add extra fields to the join (e.g., author order, primary category) in the future.

```csharp
builder.Entity<BookAuthor>().HasKey(x => new { x.BookId, x.AuthorId });
builder.Entity<BookCategory>().HasKey(x => new { x.BookId, x.CategoryId });
```

### 4. Book Availability via Status Enum

Book availability is tracked with a simple `BookStatus` enum (`In = 1`, `Out = 2`) stored directly on the `Book` entity. When a borrow transaction is created, the book's status is flipped to `Out`; when returned, it flips back to `In`. This makes availability queries trivial (a simple `WHERE Status = 1`) without requiring a join to the transactions table.

### 5. Activity Logging via a Scoped Service

Every significant user action (login, logout, book created, book borrowed, etc.) is recorded through `IActivityLogService`. This interface is registered as a **scoped** service and injected into every controller. The service writes a timestamped `ActivityLog` record to the database with the acting user's ID and a plain-text action description. Keeping logging behind an interface makes it easy to swap the implementation (e.g., write to a file or external service) without touching any controller code.

### 6. DTOs for Input, Entities for Output

Input from clients goes through DTO classes (`BookDto`, `MemberDto`, `RegisterDto`, `LoginDto`), which keeps the API contract decoupled from the internal entity shape and avoids over-posting vulnerabilities. Responses return the entity objects directly (with `[JsonIgnore]` on circular navigation properties like `BorrowTransactions` on `Book` and `Member`). A mapping library was intentionally skipped to keep the project lean.

### 7. Database Transactions in Registration

The `Register` endpoint wraps user creation and role assignment in an explicit **database transaction**. This ensures that if role assignment fails after the user is created, the partial state is rolled back — preventing orphaned user accounts with no role.

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
// ... create user, assign role ...
await transaction.CommitAsync();
```

### 8. Borrow Logic with Audit Trail

The `BorrowTransaction` entity records `BorrowDate`, `DueDate` (set to 7 days from borrow), `ReturnDate`, and `IssuedByUserId` — the ID of the staff member who processed the transaction. This gives a full audit trail of who issued or received which book, independent of the activity log.

### 9. Paginated Search

The `GET /api/books/search` endpoint supports optional filtering by title, author, and category simultaneously, with **server-side pagination** via `Skip`/`Take`. This uses `AsQueryable()` to compose the filters before hitting the database, so only one SQL query is executed regardless of which filters are active.

---

## API Endpoints

### Auth — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/register` | Public | Create a new user with a role |
| POST | `/login` | Public | Sign in (sets auth cookie) |
| POST | `/logout` | Authenticated | Sign out |

### Books — `/api/books`

| Method | Endpoint | Role Required | Description |
|---|---|---|---|
| GET | `/` | Any authenticated | Get all books (with publisher, authors, categories) |
| GET | `/{id}` | Public | Get book by ID |
| POST | `/` | Administrator, Librarian | Create a book |
| PUT | `/{id}` | Administrator, Librarian | Update a book |
| DELETE | `/{id}` | Administrator only | Delete a book |
| GET | `/search` | Public | Search by title/author/category with pagination |
| GET | `/searchByTitle` | Administrator, Librarian, Staff | Search by title |
| GET | `/searchByAuthor` | Administrator, Librarian, Staff | Search by author |
| GET | `/searchByCategory` | Administrator, Librarian, Staff | Search by category |
| GET | `/status/in` | Public | List available books |
| GET | `/status/out` | Public | List borrowed books |

### Borrow — `/api/borrow`

| Method | Endpoint | Role Required | Description |
|---|---|---|---|
| POST | `/borrow` | Administrator, Librarian, Staff | Borrow a book |
| POST | `/return` | Administrator, Librarian, Staff | Return a book |

### Members — `/api/members`

| Method | Endpoint | Role Required | Description |
|---|---|---|---|
| GET | `/` | Public | List all members |
| POST | `/` | Administrator, Librarian | Create a member |
| PUT | `/{id}` | Administrator, Librarian | Update a member |
| DELETE | `/{id}` | Public | Delete a member |

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server LocalDB

### Setup

1. **Clone the repository** and open the solution in Visual Studio or VS Code.

2. **Configure the connection string** in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LibraryDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Apply migrations** to create the database:
   ```bash
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Open Swagger UI** at `https://localhost:{port}/swagger` to explore and test the API.

On first run, the application automatically seeds the three roles (`Administrator`, `Librarian`, `Staff`) along with sample authors and categories.

### Creating Your First Admin User

Call `POST /api/auth/register` with:
```json
{
  "userName": "admin",
  "email": "admin@library.com",
  "fullName": "Admin User",
  "password": "Admin@123",
  "role": "Administrator"
}
```

Then log in via `POST /api/auth/login` to receive an auth cookie for subsequent requests.

---

## Known Limitations & Future Improvements

- **Member deletion is unprotected** — the `DELETE /api/members/{id}` endpoint currently has no `[Authorize]` attribute, which should be fixed.
- **No refresh tokens** — since the auth uses cookies, session expiry is handled by Identity defaults; this could be made configurable.
- **No fine/late fee tracking** — the `DueDate` is recorded but overdue penalties are not currently enforced.
- **No publisher CRUD endpoints** — publishers are referenced by ID when creating books but have no management endpoints.
- **Cover image support** — a `CoverImageUrl` field exists in the `Book` model but is currently commented out.
