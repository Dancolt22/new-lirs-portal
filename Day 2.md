# Day 2: Backend Development and Database Integration

This day covers the half of a system that users never see: the API, the business rules, and the database. Every concept is illustrated with a LIRS example, and the day's skills are applied to the capstone, the **LIRS Taxpayer Mini-Portal**, by building its database and API.

```
Frontend -> API -> Backend logic -> Database -> Response back up the chain
```

## 1. Frontend, backend, client, and server

**The client** is the program that asks for something.
*Example:* A taxpayer's browser asking the portal for their balance, or an officer's Postman window asking for a taxpayer's returns.

**The server** is the program that receives the request and answers it.
*Example:* The LIRS API receiving "give me taxpayer 101's balance" and sending back a number.

**The frontend** is the part of a system the user sees and clicks.
*Example:* The balance page on the taxpayer portal, with its buttons and tables.

**The backend** is the part that holds the rules, security checks, and data handling the user never sees.
*Example:* The code that decides a payment larger than the amount owed must be rejected.

**Why the backend sits between users and data.** Users never talk to the database directly, because the backend checks who they are and what they may do first.
*Example:* A taxpayer must never be able to query another taxpayer's records, and the backend is where that rule is enforced.

## 2. HTTP: how clients and servers talk

**HTTP** is the language of the web. Every exchange is one request followed by one response.
*Example:* The browser sends "GET /api/taxpayers/101/balance" and the server replies with the balance.

**A request** has a method, a URL, headers, and sometimes a body.
*Example:*
```
POST /api/payments HTTP/1.1
Content-Type: application/json

{ "returnId": 2, "amount": 150000.00, "channel": "Card" }
```

**A response** has a status code, headers, and usually a body.
*Example:*
```
HTTP/1.1 201 Created
Content-Type: application/json

{ "paymentId": 5, "receiptNumber": "RCT-000005", "status": "Successful" }
```

**A header** is extra information attached to a request or response.
*Example:* `Content-Type: application/json` says the body is JSON. On Day 3 an `Authorization` header will carry the login token.

**JSON** is a plain-text format for structured data that every programming language can read.
*Example:* `{ "tin": "1000000001", "name": "Adewale Ventures Ltd", "state": "Lagos" }`

## 3. REST APIs

**An API (Application Programming Interface)** is an agreed doorway through which programs talk to each other.
*Example:* The taxpayer portal's React screens call the LIRS API instead of reading the database themselves.

**REST** is a style of designing APIs around resources, which are things such as taxpayers or payments, using standard HTTP methods.
*Example:* `GET /api/taxpayers/101` reads taxpayer 101, and `POST /api/payments` creates a payment.

**An endpoint** is one specific URL plus method that the API offers.
*Example:* `GET /api/taxpayers/101/returns` returns taxpayer 101's tax returns.

**Resource-based URLs** use nouns, not verbs. The method says the action.
*Example:* `GET /api/taxpayers/101` is good. `/getTaxpayerById?id=101` is not.

**HTTP methods** state what the client wants to do.

| Method | Meaning | LIRS example |
|---|---|---|
| GET | Read | Get a taxpayer's returns |
| POST | Create | Record a new payment |
| PUT | Replace the whole record | Replace a taxpayer's full profile |
| PATCH | Change part of a record | Update only a phone number, or a return's status |
| DELETE | Remove | Cancel a draft return |

**Status codes** are the server's short answer about how the request went.

| Code | Meaning | LIRS example |
|---|---|---|
| 200 OK | It worked | Balance returned |
| 201 Created | A new record was made | Payment recorded |
| 400 Bad Request | The request was invalid | Payment amount of ₦0 |
| 401 Unauthorized | Not logged in | Missing token (Day 3) |
| 403 Forbidden | Logged in but not allowed | A taxpayer opening an officer report (Day 4) |
| 404 Not Found | The thing does not exist | TIN that matches nobody |
| 500 Internal Server Error | The server failed | Database unreachable |

**Routing** is how the server matches a URL and method to the code that handles it.
*Example:* `GET /api/taxpayers/{id}` routes to the method that loads one taxpayer.

**CRUD** stands for Create, Read, Update, Delete, the four basic data actions that most endpoints perform.
*Example:* Creating a payment, reading a balance, updating a return's status, deleting a draft return.

## 4. Databases

**A database** is an organized store of data that programs can query and update reliably.
*Example:* The place where every taxpayer, return, and payment of the portal lives.

**A table** holds one kind of thing, a **row** is one item in it, and a **column** is one property of that item.
*Example:* The Taxpayers table has one row per taxpayer, and columns such as TIN, Name, and State.

**A primary key (PK)** is a column whose value uniquely identifies each row and is never repeated.
*Example:* `TaxpayerId`: only one taxpayer has ID 101.

**A foreign key (FK)** is a column that points to another table's primary key, linking the two.
*Example:* `TaxReturns.TaxpayerId` points to `Taxpayers.TaxpayerId`, so every return belongs to a real taxpayer.

**A relationship** describes how tables connect.

**One-to-one.** One row in A matches one row in B.
*Example:* A taxpayer and their single verified identity record.

**One-to-many.** One row in A matches many rows in B. This is the most common kind.
*Example:* One taxpayer has many returns, and one return has many payments.

**Many-to-many.** Many rows in A match many in B, and a link table sits between them.
*Example:* Many officers review many returns, so a `ReturnReviews` table links OfficerId and ReturnId.

### The capstone data model

| Table | Columns | Relationship |
|---|---|---|
| Taxpayers | TaxpayerId (PK), TIN, Name, Type, State, Phone | One taxpayer has many returns |
| TaxReturns | ReturnId (PK), TaxpayerId (FK), TaxYear, DeclaredIncome, TaxDue, Status | One return has many payments |
| Payments | PaymentId (PK), ReturnId (FK), Amount, PaidOn, Channel | Belongs to one return |
| ComplianceLogs | LogId (PK), TaxpayerId (FK), Event, OfficerName, LoggedAt | Belongs to one taxpayer |

**Money columns use DECIMAL, never floating point.** Floating point numbers cannot store some decimal values exactly.
*Example:* ₦1,250,000.50 stored as `DECIMAL(18,2)` stays exact across millions of payments.

**Basic normalization** means storing each fact in one place and linking to it.
*Example:* The taxpayer's name is stored once in Taxpayers. A payment row holds only a return ID, so a name correction is made once and every payment screen shows it.

## 5. SQL

**SQL** is the language for asking a relational database to read or change data.
*Example:* "Give me every Lagos business taxpayer."

**SELECT** reads data, **WHERE** filters it, and **ORDER BY** sorts it.
```sql
SELECT Name, State FROM Taxpayers WHERE Type = 'Business' ORDER BY Name;
```
*Example:* A list of business taxpayers in alphabetical order.

**INSERT** adds a row.
```sql
INSERT INTO Taxpayers (TaxpayerId, TIN, Name, Type, State, Phone)
VALUES (105, '1000000005', 'Sade Bakeries', 'Business', 'Lagos', '08030000005');
```
*Example:* Registering a new taxpayer.

**UPDATE** changes existing rows.
```sql
UPDATE Taxpayers SET Phone = '08031111111' WHERE TaxpayerId = 101;
```
*Example:* A taxpayer changes their phone number.

**DELETE** removes rows.
```sql
DELETE FROM TaxReturns WHERE ReturnId = 99 AND Status = 'Draft';
```
*Example:* Cancelling a draft return.

**The missing WHERE danger.** An UPDATE or DELETE without WHERE changes every row in the table.
*Example:* `DELETE FROM Payments;` erases every payment LIRS has recorded.

**JOIN** combines rows from related tables.
**Inner join** returns only rows that match in both tables.
**Left join** returns every row from the left table, even with no match.
```sql
SELECT t.Name, r.TaxYear, r.TaxDue,
       COALESCE(SUM(p.Amount), 0) AS TotalPaid
FROM Taxpayers t
JOIN TaxReturns r ON r.TaxpayerId = t.TaxpayerId
LEFT JOIN Payments p ON p.ReturnId = r.ReturnId
GROUP BY t.Name, r.TaxYear, r.TaxDue;
```
*Example:* Every return with how much has been paid. The left join keeps returns that nobody has paid yet.

**GROUP BY and aggregate functions** (`SUM`, `COUNT`, `AVG`, `MAX`) summarize many rows into one result per group.
```sql
SELECT t.State, SUM(p.Amount) AS TotalCollected
FROM Payments p
JOIN TaxReturns r ON r.ReturnId = p.ReturnId
JOIN Taxpayers t ON t.TaxpayerId = r.TaxpayerId
GROUP BY t.State;
```
*Example:* Total collected per state, the officer collection report.

**COALESCE** replaces a missing value with a default.
*Example:* A taxpayer with no payments has no SUM, so `COALESCE(SUM(Amount), 0)` shows ₦0.00 instead of nothing.

**An index** is a structure that lets the database find rows quickly without reading the whole table, like the index at the back of a textbook.
```sql
CREATE INDEX IX_TaxReturns_TaxpayerId ON TaxReturns (TaxpayerId);
```
*Example:* Finding every return for one taxpayer among millions of rows takes milliseconds instead of seconds.

**Index trade-offs.** Index the columns that are searched, joined, or sorted often. Every extra index slows down inserts slightly.
*Example:* TIN and TaxpayerId get indexes. A rarely used Phone column does not.

**Query optimization** is making queries do less work.
- Select only the columns needed, not `SELECT *`.
- Return results in pages, not all at once.
- Filter in the database, not in application code.
- Read the query plan to find slow steps.
*Example:* An officer's taxpayer search returns 20 rows per page, not all 2 million.

**Paging** returns a limited slice of results.
```sql
SELECT TaxpayerId, TIN, Name FROM Taxpayers
ORDER BY Name
OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY;
```
*Example:* Page 1 of the officer's taxpayer list.

## 6. Structuring a backend

**Layers** separate the jobs inside a backend so each part is easy to read, test, and change.

| Layer | Job | LIRS example |
|---|---|---|
| Controller | Receives the request and returns the response. Stays thin | `PaymentsController` accepts `POST /api/payments` |
| Service | Holds the business logic and rules | `PaymentService` rejects overpayments |
| Repository | Talks to the database | `PaymentRepository` saves a payment row |

**Business logic** is the set of rules specific to what the organization does.
*Example:* "A payment cannot exceed the amount still owed on the return."

**Dependency injection (DI)** means a class receives the things it needs instead of creating them itself, which makes it easy to swap or test.
*Example:* `PaymentService` is handed a `PaymentRepository`, so a test can hand it a fake one with no database.

**Separation of concerns.** The controller does not run SQL, and the repository does not decide business rules.
*Example:* Changing the overpayment rule touches only `PaymentService`.

## 7. Validation, error handling, and logging

**Validation** is checking incoming data before using it. It always happens on the server, even when the frontend already checked, because requests can bypass a frontend.
*Example:* The payment endpoint rejects an amount of zero, a negative amount, or a channel that is not Bank, Card, or USSD.

**Error handling** means catching problems and answering with a clear, safe message.
*Example:* "Payment of ₦150,000.00 exceeds the outstanding ₦100,000.00." instead of a crash.

**Safe error messages.** Users never see stack traces or database errors, because those help attackers.
*Example:* A database failure returns "Something went wrong. Please try again." and the real error goes to the log.

**A consistent error shape** makes errors easy for the frontend to show.
*Example:* `{ "error": "Return not found.", "code": "NOT_FOUND" }`

**Logging** is recording what the system does, so problems can be investigated afterwards.
*Example:* "Payment 5 recorded for return 2." and, on failure, the error with the return ID.

**Log levels** rank how serious a log entry is: Debug, Information, Warning, Error, Critical.
*Example:* A successful payment is Information. A gateway timeout is Error.

**What logs must never contain:** passwords, tokens, full card numbers, or anything that would harm a taxpayer if the log leaked.

**An audit trail** is a permanent record of who did what and when for sensitive actions.
*Example:* The ComplianceLogs table records "Return 3 approved" with the officer's name and the time.

## 8. Secure data access

**SQL injection** is an attack in which someone types SQL into a form field and tricks the database into running it.
*Example:* Typing `' OR '1'='1` into a TIN field to make the system return every taxpayer.

**A parameterized query** sends the SQL and the values separately, so the values can never be run as SQL. This is the main defence.
```csharp
// Unsafe: the TIN is glued into the SQL text
var sql = "SELECT * FROM Taxpayers WHERE TIN = '" + tin + "'";

// Safe: the TIN is a parameter
var taxpayer = await db.QuerySingleOrDefaultAsync<Taxpayer>(
    "SELECT * FROM Taxpayers WHERE TIN = @Tin", new { Tin = tin });
```
*Example:* The safe version treats `' OR '1'='1` as an ordinary, non-matching TIN.

**Least privilege** means giving a database account only the permissions it needs.
*Example:* The API's database account can read and write its own tables but cannot drop tables or read other systems' data.

**Secrets management.** Passwords, keys, and connection strings with passwords stay out of source code and out of Git.
*Example:* The database password lives in an environment variable or in `dotnet user-secrets`, never in a committed file.

## 9. Performance for high-volume systems

**High volume** means many requests and large tables, where small inefficiencies multiply.
*Example:* Month-end filing deadlines, when thousands of taxpayers hit the portal in the same hour.

**The N+1 problem** is running one query per row inside a loop, instead of one query for all rows.
*Example:* Loading 50 taxpayers and then running 50 separate queries for their balances, instead of one JOIN.

**Async code** lets the server handle other requests while it waits for the database.
*Example:* `await` in C# frees the thread during a slow query, so more taxpayers can be served at once.

**Caching** keeps results that rarely change so they are not fetched again every time.
*Example:* The list of Nigerian states used in a dropdown is loaded once, not on every page view.

## 10. The technology used on Day 2

**ASP.NET Core** is Microsoft's framework for building web APIs in C#.
*Example:* The capstone API is an ASP.NET Core project with controllers, services, and repositories.

**C#** is the main language of the .NET ecosystem, widely used in government and enterprise systems.
*Example:* The same language runs the API, the business rules, and the unit tests.

**Python with FastAPI** is a lighter way to build an API in Python.
```python
from typing import Literal
from fastapi import FastAPI
from pydantic import BaseModel, Field

app = FastAPI()

class PaymentRequest(BaseModel):
    return_id: int
    amount: float = Field(gt=0)                       # must be above zero
    channel: Literal["Bank", "Card", "USSD"]

@app.post("/api/payments", status_code=201)
def create_payment(request: PaymentRequest):
    return {"payment_id": 5, "status": "Successful"}
```
*Example:* The same payment rule written in Python. Invalid amounts or channels are rejected with a 422 error automatically.

**Dapper** is a small C# library that runs SQL queries and maps rows to C# objects.
*Example:* `QuerySingleOrDefaultAsync<Taxpayer>(sql, parameters)` runs a parameterized query and returns a `Taxpayer`.

**SQL Server and PostgreSQL** are two widely used relational databases. Their SQL is mostly the same.

| Difference | SQL Server | PostgreSQL |
|---|---|---|
| Auto-numbered ID column | `INT IDENTITY(1,1) PRIMARY KEY` | `INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY` |
| Date and time type | `DATETIME2` | `TIMESTAMP` |
| Get the new row's ID | `OUTPUT INSERTED.PaymentId` | `RETURNING PaymentId` |
| Current time | `SYSDATETIME()` | `NOW()` |
| .NET connection package | `Microsoft.Data.SqlClient` | `Npgsql` |

**Postman** is a tool for sending requests to an API and reading the responses, with no frontend needed.
*Example:* Testing `POST /api/payments` with a JSON body before the React form exists.

## Capstone work for Day 2

By the end of Day 2 the capstone has a real database and a working API for taxpayers, returns, balances, and payments. There is no screen yet. The API is tested through Postman, and the endpoints have no login until Days 3 and 4.

### The repository

```
lirs-taxpayer-portal/
  docs/
  backend/
    database/
      01_schema.sql
      02_seed.sql
    LirsPortal.Api/
      Controllers/
        TaxpayersController.cs
        PaymentsController.cs
      Services/
        PaymentService.cs
      Repositories/
        TaxpayerRepository.cs
        PaymentRepository.cs
      Models/
        Models.cs
      Data/
        DbConnectionFactory.cs
      Exceptions.cs
      Program.cs
      appsettings.json
    starter/
  frontend/
```

### Database script: `backend/database/01_schema.sql` (SQL Server)

```sql
CREATE DATABASE LirsPortal;
GO
USE LirsPortal;
GO

CREATE TABLE Taxpayers (
    TaxpayerId INT PRIMARY KEY,
    TIN        VARCHAR(10)  NOT NULL UNIQUE,
    Name       VARCHAR(150) NOT NULL,
    Type       VARCHAR(20)  NOT NULL,   -- Individual or Business
    State      VARCHAR(50)  NOT NULL,
    Phone      VARCHAR(20)
);

CREATE TABLE TaxReturns (
    ReturnId       INT PRIMARY KEY,
    TaxpayerId     INT NOT NULL REFERENCES Taxpayers (TaxpayerId),
    TaxYear        INT NOT NULL,
    DeclaredIncome DECIMAL(18,2) NOT NULL,
    TaxDue         DECIMAL(18,2) NOT NULL,
    Status         VARCHAR(20) NOT NULL  -- Draft, Submitted, Approved
);

CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    ReturnId  INT NOT NULL REFERENCES TaxReturns (ReturnId),
    Amount    DECIMAL(18,2) NOT NULL,
    PaidOn    DATETIME2 NOT NULL,
    Channel   VARCHAR(20) NOT NULL       -- Bank, Card, USSD
);

CREATE TABLE ComplianceLogs (
    LogId       INT IDENTITY(1,1) PRIMARY KEY,
    TaxpayerId  INT NOT NULL REFERENCES Taxpayers (TaxpayerId),
    Event       VARCHAR(200) NOT NULL,
    OfficerName VARCHAR(100),
    LoggedAt    DATETIME2 NOT NULL
);

CREATE INDEX IX_TaxReturns_TaxpayerId ON TaxReturns (TaxpayerId);
CREATE INDEX IX_Payments_ReturnId     ON Payments (ReturnId);
```

### Fictional data: `backend/database/02_seed.sql`

```sql
USE LirsPortal;
GO

INSERT INTO Taxpayers (TaxpayerId, TIN, Name, Type, State, Phone) VALUES
(101, '1000000001', 'Adewale Ventures Ltd', 'Business',   'Lagos', '08030000001'),
(102, '1000000002', 'Chioma Okafor',        'Individual', 'Lagos', '08030000002'),
(103, '1000000003', 'Bello Logistics',      'Business',   'Lagos', '08030000003'),
(104, '1000000004', 'Ngozi Textiles',       'Business',   'Ogun',  '08030000004');

INSERT INTO TaxReturns (ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status) VALUES
(1, 101, 2025,  5000000.00,  500000.00, 'Approved'),
(2, 101, 2026,  6000000.00,  600000.00, 'Submitted'),
(3, 102, 2025,  2400000.00,  240000.00, 'Approved'),
(4, 103, 2025, 10000000.00, 1000000.00, 'Approved'),
(5, 104, 2025,  3000000.00,  300000.00, 'Submitted');

INSERT INTO Payments (ReturnId, Amount, PaidOn, Channel) VALUES
(1, 300000.00, '2026-03-10', 'Bank'),
(1, 100000.00, '2026-05-02', 'Card'),
(3, 240000.00, '2026-04-15', 'USSD'),
(4, 400000.00, '2026-06-01', 'Bank');

INSERT INTO ComplianceLogs (TaxpayerId, Event, OfficerName, LoggedAt) VALUES
(101, 'Return 1 approved', 'Officer Bisi', '2026-02-20'),
(103, 'Return 4 approved', 'Officer Bisi', '2026-03-05');
```

Expected balances from this data (total tax due on non-draft returns minus total paid):

| Taxpayer | Tax due | Paid | Balance |
|---|---|---|---|
| 101 Adewale Ventures Ltd | ₦1,100,000.00 | ₦400,000.00 | ₦700,000.00 |
| 102 Chioma Okafor | ₦240,000.00 | ₦240,000.00 | ₦0.00 |
| 103 Bello Logistics | ₦1,000,000.00 | ₦400,000.00 | ₦600,000.00 |
| 104 Ngozi Textiles | ₦300,000.00 | ₦0.00 | ₦300,000.00 |

### Creating the API project

```bash
cd backend
dotnet new webapi --use-controllers -n LirsPortal.Api
cd LirsPortal.Api
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
```

Remove the template's sample weather controller and model files. For PostgreSQL, add `Npgsql` instead of `Microsoft.Data.SqlClient`, and use `NpgsqlConnection` in the connection factory.

### Connection string: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "LirsDb": "Server=localhost\\SQLEXPRESS;Database=LirsPortal;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

This uses Windows authentication, so no password sits in the file. A connection string with a password goes into user secrets instead:
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:LirsDb" "Server=...;Password=..."
```

### Models and exceptions: `Models/Models.cs` and `Exceptions.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace LirsPortal.Api.Models;

public class Taxpayer
{
    public int TaxpayerId { get; set; }
    public string TIN { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string State { get; set; } = "";
    public string? Phone { get; set; }
}

public class TaxReturn
{
    public int ReturnId { get; set; }
    public int TaxpayerId { get; set; }
    public int TaxYear { get; set; }
    public decimal DeclaredIncome { get; set; }
    public decimal TaxDue { get; set; }
    public string Status { get; set; } = "";
}

public class PaymentRequest
{
    [Required]
    public int ReturnId { get; set; }

    [Range(typeof(decimal), "0.01", "1000000000", ErrorMessage = "Amount must be above zero.")]
    public decimal Amount { get; set; }

    [Required, RegularExpression("^(Bank|Card|USSD)$", ErrorMessage = "Channel must be Bank, Card, or USSD.")]
    public string Channel { get; set; } = "";
}

public record PaymentResult(int PaymentId, string ReceiptNumber, string Status);
public record BalanceResult(int TaxpayerId, decimal Balance);
public record ApiError(string Error, string Code);
```

```csharp
// Exceptions.cs
namespace LirsPortal.Api;

public class NotFoundException(string message) : Exception(message);
public class BusinessRuleException(string message) : Exception(message);
```

### Data access: `Data/DbConnectionFactory.cs` and the repositories

```csharp
// Data/DbConnectionFactory.cs
using System.Data;
using Microsoft.Data.SqlClient;

namespace LirsPortal.Api.Data;

public class DbConnectionFactory(IConfiguration config)
{
    public IDbConnection Create() => new SqlConnection(config.GetConnectionString("LirsDb"));
}
```

```csharp
// Repositories/TaxpayerRepository.cs
using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

public class TaxpayerRepository(DbConnectionFactory factory)
{
    public async Task<IEnumerable<Taxpayer>> GetPageAsync(string? tin, int page, int pageSize)
    {
        const string sql = @"
            SELECT TaxpayerId, TIN, Name, Type, State, Phone
            FROM Taxpayers
            WHERE (@Tin IS NULL OR TIN = @Tin)
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        using var db = factory.Create();
        return await db.QueryAsync<Taxpayer>(sql, new { Tin = tin, Offset = (page - 1) * pageSize, PageSize = pageSize });
    }

    public async Task<Taxpayer?> GetByIdAsync(int id)
    {
        const string sql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TaxpayerId = @Id";
        using var db = factory.Create();
        return await db.QuerySingleOrDefaultAsync<Taxpayer>(sql, new { Id = id });
    }

    public async Task<IEnumerable<TaxReturn>> GetReturnsAsync(int taxpayerId)
    {
        const string sql = @"
            SELECT ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status
            FROM TaxReturns
            WHERE TaxpayerId = @TaxpayerId
            ORDER BY TaxYear DESC";
        using var db = factory.Create();
        return await db.QueryAsync<TaxReturn>(sql, new { TaxpayerId = taxpayerId });
    }

    public async Task<decimal> GetBalanceAsync(int taxpayerId)
    {
        const string sql = @"
            SELECT
              COALESCE((SELECT SUM(TaxDue) FROM TaxReturns
                        WHERE TaxpayerId = @TaxpayerId AND Status <> 'Draft'), 0)
              -
              COALESCE((SELECT SUM(p.Amount) FROM Payments p
                        JOIN TaxReturns r ON r.ReturnId = p.ReturnId
                        WHERE r.TaxpayerId = @TaxpayerId), 0);";
        using var db = factory.Create();
        return await db.QuerySingleAsync<decimal>(sql, new { TaxpayerId = taxpayerId });
    }
}
```

```csharp
// Repositories/PaymentRepository.cs
using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

public class PaymentRepository(DbConnectionFactory factory)
{
    public async Task<TaxReturn?> GetReturnAsync(int returnId)
    {
        const string sql = @"SELECT ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status
                             FROM TaxReturns WHERE ReturnId = @ReturnId";
        using var db = factory.Create();
        return await db.QuerySingleOrDefaultAsync<TaxReturn>(sql, new { ReturnId = returnId });
    }

    public async Task<decimal> GetTotalPaidAsync(int returnId)
    {
        const string sql = "SELECT COALESCE(SUM(Amount), 0) FROM Payments WHERE ReturnId = @ReturnId";
        using var db = factory.Create();
        return await db.QuerySingleAsync<decimal>(sql, new { ReturnId = returnId });
    }

    public async Task<int> SaveAsync(PaymentRequest request)
    {
        const string sql = @"
            INSERT INTO Payments (ReturnId, Amount, PaidOn, Channel)
            OUTPUT INSERTED.PaymentId
            VALUES (@ReturnId, @Amount, SYSDATETIME(), @Channel);";
        using var db = factory.Create();
        return await db.ExecuteScalarAsync<int>(sql, request);
    }
}
```

### Business logic: `Services/PaymentService.cs`

```csharp
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;

namespace LirsPortal.Api.Services;

public class PaymentService(PaymentRepository payments, ILogger<PaymentService> logger)
{
    public async Task<PaymentResult> RecordPaymentAsync(PaymentRequest request)
    {
        var taxReturn = await payments.GetReturnAsync(request.ReturnId)
            ?? throw new NotFoundException("Return not found.");

        if (taxReturn.Status == "Draft")
            throw new BusinessRuleException("Payments cannot be recorded against a draft return.");

        var alreadyPaid = await payments.GetTotalPaidAsync(request.ReturnId);
        var outstanding = taxReturn.TaxDue - alreadyPaid;

        // A payment must never exceed what is still owed on the return
        if (request.Amount > outstanding)
            throw new BusinessRuleException(
                $"Payment of {request.Amount:N2} exceeds the outstanding {outstanding:N2}.");

        var paymentId = await payments.SaveAsync(request);
        logger.LogInformation("Payment {PaymentId} recorded for return {ReturnId}", paymentId, request.ReturnId);

        return new PaymentResult(paymentId, $"RCT-{paymentId:D6}", "Successful");
    }
}
```

### Controllers

```csharp
// Controllers/TaxpayersController.cs
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxpayersController(TaxpayerRepository repo) : ControllerBase
{
    // GET /api/taxpayers?tin=1000000001&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetAll(string? tin, int page = 1, int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await repo.GetPageAsync(tin, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var taxpayer = await repo.GetByIdAsync(id);
        return taxpayer is null
            ? NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"))
            : Ok(taxpayer);
    }

    [HttpGet("{id}/returns")]
    public async Task<IActionResult> GetReturns(int id)
    {
        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));
        return Ok(await repo.GetReturnsAsync(id));
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));
        return Ok(new BalanceResult(id, await repo.GetBalanceAsync(id)));
    }
}
```

```csharp
// Controllers/PaymentsController.cs
using LirsPortal.Api.Models;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(PaymentService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(PaymentRequest request)
    {
        try
        {
            var result = await service.RecordPaymentAsync(request);
            return Created($"/api/payments/{result.PaymentId}", result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiError(ex.Message, "NOT_FOUND"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new ApiError(ex.Message, "BUSINESS_RULE"));
        }
    }
}
```

Add `using LirsPortal.Api;` to the top of `PaymentsController.cs` so the exception classes are found.

### Wiring it together: `Program.cs`

```csharp
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<TaxpayerRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

// Any unexpected failure returns a safe, consistent message; details go to the log
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(new ApiError("Something went wrong. Please try again.", "SERVER_ERROR"));
}));

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

Run it with `dotnet run`. The console shows the local address, for example `http://localhost:5000`. Use whichever port it prints.

### Postman requests for the capstone

| Request | Expected result |
|---|---|
| `GET /api/taxpayers` | 200 with four taxpayers, sorted by name |
| `GET /api/taxpayers?tin=1000000001` | 200 with Adewale Ventures Ltd only |
| `GET /api/taxpayers/101` | 200 with one taxpayer |
| `GET /api/taxpayers/999` | 404 `Taxpayer not found.` |
| `GET /api/taxpayers/101/returns` | 200 with two returns, newest year first |
| `GET /api/taxpayers/101/balance` | 200 `{ "taxpayerId": 101, "balance": 700000.00 }` |
| `POST /api/payments` with `{ "returnId": 2, "amount": 150000.00, "channel": "Card" }` | 201 with a receipt number |
| `GET /api/taxpayers/101/balance` again | balance now 550000.00 |
| `POST /api/payments` with amount `0` | 400 from validation |
| `POST /api/payments` with `"channel": "Cash"` | 400 from validation |
| `POST /api/payments` with `{ "returnId": 5, "amount": 999999999.00, "channel": "Bank" }` | 400 `exceeds the outstanding` |
| `POST /api/payments` with `{ "returnId": 99, ... }` | 404 `Return not found.` |

The requests are saved as a Postman collection and exported to `backend/postman/LirsPortal.postman_collection.json`.

### Stories delivered and Git work

| Story | Delivered by |
|---|---|
| US-02 Taxpayer balance | `GET /api/taxpayers/{id}/balance` |
| US-03 Taxpayer returns | `GET /api/taxpayers/{id}/returns` |
| US-04 Taxpayer payment | `POST /api/payments` with validation and the overpayment rule |
| US-05 Officer search by TIN | `GET /api/taxpayers?tin=...` with paging |

Git branches follow the story names, each merged through a pull request:

| Branch | Contents |
|---|---|
| `feature/database` | `01_schema.sql`, `02_seed.sql` |
| `feature/api-setup` | Project, models, exceptions, connection factory, `Program.cs` |
| `feature/US-02-US-03-US-05-taxpayer-endpoints` | `TaxpayerRepository`, `TaxpayersController` |
| `feature/US-04-payments` | `PaymentRepository`, `PaymentService`, `PaymentsController` |
| `feature/postman-collection` | The exported Postman collection |

The `.gitignore` already excludes `bin/`, `obj/`, and `.env`, so build output and secrets stay out of the repository.

### State of the capstone at the end of Day 2

| Deliverable | Status |
|---|---|
| Database schema, indexes, and fictional data | Complete |
| Taxpayer, returns, balance, and TIN search endpoints | Complete |
| Payment endpoint with validation and business rule | Complete |
| Consistent error shape and safe error messages | Complete |
| Parameterized queries throughout | Complete |
| Postman collection committed | Complete |
| Login and role checks | Days 3 and 4 |
| React screens | Day 3 |
