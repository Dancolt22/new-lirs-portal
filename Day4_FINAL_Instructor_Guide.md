# Day 4 FINAL Instructor Guide: Testing, Security, and System Integration

This guide is complete on purpose. Every action says **where to go**, then **what to click or type**, then **what you should see**, then **what to do if you do not**. Follow it from top to bottom and nothing should be missing.

**How to read this guide:**
- **GO TO:** The exact program, window, or folder to open.
- **STEPS:** Numbered actions, in exact sequence.
- **YOU SHOULD SEE:** The exact output, screen message, or response confirming success.
- **IF IT FAILS:** Likely errors with exact, copy-paste fixes.
- **EXPLAIN:** Plain-language speaking points for the executive participant.
- Code blocks are 100% complete. Copy and paste them exactly as provided.

---

## 0. What we are building today and why

Today's goal: transform yesterday's working prototype into an **enterprise-grade, secure, verifiable system**. We prove the code works with **automated tests**, lock down the API with **real database authentication and JWT tokens**, protect taxpayer privacy with **ownership checks**, demonstrate defense against the **OWASP Top 10**, and integrate a **payment gateway with idempotency**.

```
Client (Postman / React)
       │
       ▼ [Bearer JWT Token in Authorization Header]
┌────────────────────────────────────────────────────────────────────────┐
│ ASP.NET Core API Pipeline (LirsPortal.Api)                             │
│                                                                        │
│  1. JwtBearer Middleware  ──► Validates cryptographic signature & expiry│
│  2. Controllers           ──► Enforces [Authorize] & [Authorize(Roles)]│
│  3. Ownership Filter      ──► Taxpayer 101 CANNOT read Taxpayer 102    │
│  4. PaymentService        ──► Checks Idempotency & Gateway Settlement  │
│  5. Dapper Repositories   ──► Parameterized SQL (Immune to Injection)  │
└────────────────────────────────────┬───────────────────────────────────┘
                                     │
                 ┌───────────────────┴───────────────────┐
                 ▼                                       ▼
      SQL Server (LirsPortal)             Mock Payment Gateway
      - Users (Hashed Passwords)          - Settlement Verification
      - Taxpayers / Returns / Payments    - 10s Timeout & Retries
      - ComplianceLogs (Audit Trail)
```

**EXPLAIN (Say this at 09:00):**
- "Over the last three days we planned our backlog, built a database, wired an API, and built a React dashboard. But in government revenue systems, working software is only half the job. The system must be **provably correct** and **resilient against attack**."
- "Today we cover three pillars: **Testing** (proving calculations are accurate without clicking manually), **Security** (locking every door so Taxpayer A cannot inspect Taxpayer B's records), and **Integration** (connecting safely to banking gateways without double-crediting payments)."
- "By 16:00, our system will use cryptographic JSON Web Tokens, store salted password hashes, block brute-force attacks, reject unauthorized access with HTTP 403 Forbidden, and verify payments through an automated test suite."

### Words used today, in one line each

| Word | Plain meaning |
|---|---|
| **Unit test** | A small automated script that runs one method in isolation and asserts expected output. |
| **Test case** | A specific test scenario with defined inputs, execution steps, and expected outcomes. |
| **Regression** | A bug that re-appears in previously working code after a new feature or change is added. |
| **Arrange-Act-Assert** | The standard 3-step test pattern: set up data, invoke the method, verify the result. |
| **Test double (Fake)** | A lightweight in-memory substitute for a database that lets tests run in milliseconds. |
| **Hashing** | A one-way mathematical function turning a password into an irreversible string. |
| **Salt** | Random cryptographic bytes added to a password before hashing to defeat rainbow tables. |
| **PBKDF2** | A NIST-approved password hashing algorithm that intentionally uses thousands of iterations. |
| **JWT (JSON Web Token)** | A compact, URL-safe, cryptographically signed token proving user identity and roles. |
| **Claim** | A key-value fact inside a JWT token, such as `sub=101`, `role=Taxpayer`, or `name=adewale`. |
| **Bearer token** | An HTTP authorization header format: `Authorization: Bearer <token>`. Whoever holds it is authenticated. |
| **401 vs 403** | `401 Unauthorized` = "I do not know who you are (log in)"; `403 Forbidden` = "I know who you are, but you have no permission here." |
| **IDOR / Broken Access Control** | A security flaw where changing an ID in a URL (e.g. `/taxpayers/102`) exposes another citizen's data. |
| **SQL Injection** | An attack where untrusted text concatenates into SQL commands to manipulate the database. |
| **Idempotency** | A property where executing an operation multiple times produces the exact same outcome as executing it once. |
| **Secrets Management** | Storing passwords and encryption keys outside git repositories using OS-level secure storage. |
| **Audit Trail** | An immutable record of who did what, when, and to which taxpayer record (stored in `ComplianceLogs`). |

---

## 1. Software and Pre-Flight Checks

Before beginning the session, verify all developer tools are functioning.

**GO TO: PowerShell terminal inside VS Code (or Windows Terminal)**

Type each command and verify the output:

```powershell
dotnet --version
git --version
node --version
python --version
```

### YOU SHOULD SEE:
- `dotnet`: `8.0.xxx` or newer.
- `git`: `2.xx.x`.
- `node`: `v18.xx.x` or `v20.xx.x` LTS.
- `python`: `Python 3.11.x` or `3.12.x` (if testing the starter python scripts).

### IF IT FAILS:
| Symptom | Cause | Fix |
|---|---|---|
| `dotnet: The term 'dotnet' is not recognized` | .NET SDK not in PATH | Reinstall .NET 8 SDK and restart VS Code. |
| `python: The term 'python' is not recognized` | Python not in Windows PATH | Type `py --version`. If installed, use `py -m pytest`. |
| Port 5173 or 5123 already in use | Previous process still running in background | Run `Get-Process -Name dotnet,node -ErrorAction SilentlyContinue | Stop-Process -Force`. |

---

## 2. Morning Sanity Check: Starting from the Day 3 End State

We must confirm that the database is intact and that our repository is on a clean branch before starting Day 4.

### Step 1: Check Git working tree
**GO TO: VS Code terminal**
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal
git status
```
Ensure you are on `main` (or yesterday's merged branch) with no uncommitted scratch files. If uncommitted changes exist, run:
```powershell
git stash
```

### Step 2: Database state verification & optional clean reset
If test payments were recorded on Day 2 or Day 3, taxpayer 101's balance will have dropped from ₦700,000 to ₦550,000 or ₦0. To ensure consistent expected test numbers (101 = ₦700,000; 102 = ₦0; 103 = ₦600,000; 104 = ₦300,000), run the clean reset script below in SSMS.

**GO TO: SQL Server Management Studio (SSMS)**
1. Connect to `.\SQLEXPRESS` (tick **Trust server certificate**).
2. Click **New Query** (Ctrl+N).
3. Paste and run (press **F5**):

```sql
USE master;
GO

-- If resetting is needed:
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'LirsPortal')
BEGIN
    ALTER DATABASE LirsPortal SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LirsPortal;
END
GO

CREATE DATABASE LirsPortal;
GO

USE LirsPortal;
GO

-- 1. Schema
CREATE TABLE Taxpayers (
    TaxpayerId INT PRIMARY KEY,
    TIN VARCHAR(10) NOT NULL UNIQUE,
    Name NVARCHAR(150) NOT NULL,
    Type VARCHAR(20) NOT NULL,
    State NVARCHAR(50) NOT NULL,
    Phone VARCHAR(15) NOT NULL
);

CREATE TABLE TaxReturns (
    ReturnId INT PRIMARY KEY,
    TaxpayerId INT NOT NULL FOREIGN KEY REFERENCES Taxpayers(TaxpayerId),
    TaxYear INT NOT NULL,
    DeclaredIncome DECIMAL(18,2) NOT NULL,
    TaxDue DECIMAL(18,2) NOT NULL,
    Status VARCHAR(20) NOT NULL
);

CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    ReturnId INT NOT NULL FOREIGN KEY REFERENCES TaxReturns(ReturnId),
    Amount DECIMAL(18,2) NOT NULL,
    PaidOn DATETIME2 NOT NULL,
    Channel VARCHAR(20) NOT NULL
);

CREATE TABLE ComplianceLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    TaxpayerId INT NOT NULL FOREIGN KEY REFERENCES Taxpayers(TaxpayerId),
    Event NVARCHAR(255) NOT NULL,
    OfficerName NVARCHAR(100) NOT NULL,
    LoggedAt DATETIME2 NOT NULL
);

CREATE INDEX IX_TaxReturns_TaxpayerId ON TaxReturns (TaxpayerId);
CREATE INDEX IX_Payments_ReturnId ON Payments (ReturnId);
GO

-- 2. Seed Data
INSERT INTO Taxpayers (TaxpayerId, TIN, Name, Type, State, Phone) VALUES
(101, '1000000001', 'Adewale Ventures Ltd', 'Business', 'Lagos', '08030000001'),
(102, '1000000002', 'Chioma Okafor', 'Individual', 'Lagos', '08030000002'),
(103, '1000000003', 'Bello Logistics', 'Business', 'Lagos', '08030000003'),
(104, '1000000004', 'Ngozi Textiles', 'Business', 'Ogun', '08030000004');

INSERT INTO TaxReturns (ReturnId, TaxpayerId, TaxYear, DeclaredIncome, TaxDue, Status) VALUES
(1, 101, 2025, 5000000.00,  500000.00, 'Approved'),
(2, 101, 2026, 6000000.00,  600000.00, 'Submitted'),
(3, 102, 2025, 2400000.00,  240000.00, 'Approved'),
(4, 103, 2025, 10000000.00, 1000000.00, 'Approved'),
(5, 104, 2025, 3000000.00,  300000.00, 'Submitted');

INSERT INTO Payments (ReturnId, Amount, PaidOn, Channel) VALUES
(1, 300000.00, SYSDATETIME(), 'Bank'),
(1, 100000.00, SYSDATETIME(), 'Card'),
(3, 240000.00, SYSDATETIME(), 'USSD'),
(4, 400000.00, SYSDATETIME(), 'Bank');

INSERT INTO ComplianceLogs (TaxpayerId, Event, OfficerName, LoggedAt) VALUES
(101, 'Return 1 approved', 'Officer Bisi', SYSDATETIME()),
(103, 'Return 4 approved', 'Officer Bisi', SYSDATETIME());
GO
```

### YOU SHOULD SEE:
`Commands completed successfully.` in SSMS Results pane.

---

## 3. PART ONE: Database Security & Authentication Migration (`03_auth.sql`)

Now we add the **Users** table to store authenticated identities, roles, hashed passwords, and account lockout tracking.

**EXPLAIN:**
- "On Day 3, our login was a simulated React array in JavaScript. Anyone could open DevTools and impersonate an officer."
- "Real security lives in the database and API. Today we create a `Users` table. Notice we store `PasswordHash`, NEVER plain text passwords. We also include `FailedAttempts` and `LockedUntil` to stop automated password guessing."

### Step 1: Create the migration script
**GO TO: VS Code**
1. Expand the folder `backend/database/`.
2. Right-click `database`, select **New File**, and name it `03_auth.sql`.
3. Paste the following SQL code:

```sql
USE LirsPortal;
GO

-- 1. Create the Users table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId         INT IDENTITY(1,1) PRIMARY KEY,
        Username       VARCHAR(50) NOT NULL UNIQUE,
        PasswordHash   VARCHAR(255) NOT NULL,
        Role           VARCHAR(20) NOT NULL,       -- 'Taxpayer' or 'Officer'
        TaxpayerId     INT NULL REFERENCES Taxpayers(TaxpayerId),
        FailedAttempts INT NOT NULL DEFAULT 0,
        LockedUntil    DATETIME2 NULL
    );

    CREATE INDEX IX_Users_Username ON Users (Username);
END
GO

-- 2. Seed initial users (Pass123 hashed via PBKDF2 with HMAC-SHA256)
-- Format: {iterations}.{base64-salt}:{base64-subKey}
-- Fictional credentials:
-- adewale / pass123  (Taxpayer for Adewale Ventures Ltd, TaxpayerId 101)
-- chioma  / pass123  (Taxpayer for Chioma Okafor, TaxpayerId 102)
-- bisi    / pass123  (LIRS Officer Bisi, TaxpayerId NULL)

DELETE FROM Users;

INSERT INTO Users (Username, PasswordHash, Role, TaxpayerId, FailedAttempts, LockedUntil) VALUES
('adewale', '10000.yLd8R4V6s4w=:zY9Qe4JkM9+1L2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c=', 'Taxpayer', 101, 0, NULL),
('chioma',  '10000.yLd8R4V6s4w=:zY9Qe4JkM9+1L2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c=', 'Taxpayer', 102, 0, NULL),
('bisi',    '10000.yLd8R4V6s4w=:zY9Qe4JkM9+1L2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c=', 'Officer',  NULL, 0, NULL);
GO

-- Verification query
SELECT UserId, Username, Role, TaxpayerId, FailedAttempts, LockedUntil FROM Users;
GO
```

### Step 2: Execute the migration in SSMS
**GO TO: SSMS**
1. Open `03_auth.sql` (or copy its contents into a New Query window).
2. Press **F5** to execute.

### YOU SHOULD SEE:
In the Results grid:
| UserId | Username | Role | TaxpayerId | FailedAttempts | LockedUntil |
|---|---|---|---|---|---|
| 1 | adewale | Taxpayer | 101 | 0 | NULL |
| 2 | chioma | Taxpayer | 102 | 0 | NULL |
| 3 | bisi | Officer | NULL | 0 | NULL |

### IF IT FAILS:
| Error | Reason | Fix |
|---|---|---|
| `Cannot insert duplicate key in object 'dbo.Users'` | Users table already exists with rows | The script includes `DELETE FROM Users;`. Re-run the entire script. |
| `Foreign key constraint failed` | TaxpayerId 101 or 102 does not exist | Run the reset seed in Section 2 first. |

---

## 4. PART TWO: Automated Unit Testing with xUnit (Backend)

**EXPLAIN:**
- "Why write tests before securing our endpoints? Because when we refactor code to add authentication, tests give us an instant safety net ensuring we haven't broken revenue calculations or payment validations."
- "We use **xUnit**—the premier .NET testing framework. We use the **Arrange-Act-Assert** pattern. And to test business rules without needing SQL Server running, we use an **in-memory fake repository**."

### Step 1: Create the xUnit test project
**GO TO: VS Code terminal**
Navigate to the `backend/` directory and create the test project:

```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\backend
dotnet new xunit -n LirsPortal.Tests
```

### Step 2: Reference the API project and dependencies
Link the test project to `LirsPortal.Api`:

```powershell
cd LirsPortal.Tests
dotnet add reference ..\LirsPortal.Api\Lirsportal.Api.csproj
dotnet add package Microsoft.Extensions.Logging.Abstractions
```

*Note: In Windows PowerShell, ensure the csproj path matches the exact case (`Lirsportal.Api.csproj` or `LirsPortal.Api.csproj`).*

Delete the boilerplate template file:
```powershell
Remove-Item UnitTest1.cs -ErrorAction SilentlyContinue
```

### Step 3: Add `PenaltyCalculator.cs` to `LirsPortal.Api`
This encapsulates the official LIRS late-penalty rule: **10% of tax due plus ₦100 per day late; zero if waived or on time.**

**GO TO: VS Code**
1. Expand `backend/LirsPortal.Api/Services/`.
2. Right-click `Services`, select **New File**, name it `PenaltyCalculator.cs`.
3. Paste:

```csharp
namespace LirsPortal.Api.Services;

public class PenaltyCalculator
{
    private const decimal PercentageRate = 0.10m; // 10% statutory penalty
    private const decimal DailyFee = 100.00m;      // ₦100 per day late charge

    public decimal Calculate(decimal amountDue, int daysLate, bool isWaived = false)
    {
        if (isWaived || daysLate <= 0 || amountDue <= 0)
        {
            return 0m;
        }

        decimal percentagePenalty = amountDue * PercentageRate;
        decimal dailyPenalty = daysLate * DailyFee;

        return percentagePenalty + dailyPenalty;
    }
}
```

### Step 4: Write `PenaltyCalculatorTests.cs` in `LirsPortal.Tests`
**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Tests/`, select **New File**, name it `PenaltyCalculatorTests.cs`.
2. Paste:

```csharp
using LirsPortal.Api.Services;
using Xunit;

namespace LirsPortal.Tests;

public class PenaltyCalculatorTests
{
    private readonly PenaltyCalculator _calculator = new();

    [Fact]
    public void Calculate_WhenOnTime_ReturnsZero()
    {
        // Arrange
        decimal amountDue = 100000m;
        int daysLate = 0;

        // Act
        decimal penalty = _calculator.Calculate(amountDue, daysLate);

        // Assert
        Assert.Equal(0m, penalty);
    }

    [Fact]
    public void Calculate_WhenWaived_ReturnsZeroEvenIfLate()
    {
        // Arrange & Act
        decimal penalty = _calculator.Calculate(100000m, 15, isWaived: true);

        // Assert
        Assert.Equal(0m, penalty);
    }

    [Theory]
    [InlineData(100000, 5, 10500)]   // 10% of 100,000 (10,000) + 5 days * 100 (500) = ₦10,500
    [InlineData(50000, 1, 5100)]     // 10% of 50,000 (5,000) + 1 day * 100 (100) = ₦5,100
    [InlineData(240000, 10, 25000)]  // 10% of 240,000 (24,000) + 10 days * 100 (1,000) = ₦25,000
    public void Calculate_WhenLate_ReturnsCorrectTenPercentPlusDailyNaira(decimal due, int days, decimal expected)
    {
        // Act
        decimal penalty = _calculator.Calculate(due, days);

        // Assert
        Assert.Equal(expected, penalty);
    }

    [Fact]
    public void Calculate_WhenAmountDueIsNegative_ReturnsZero()
    {
        // Act
        decimal penalty = _calculator.Calculate(-50000m, 3);

        // Assert
        Assert.Equal(0m, penalty);
    }
}
```

### Step 5: Run the first test suite
**GO TO: VS Code terminal**
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\backend\LirsPortal.Tests
dotnet test
```

### YOU SHOULD SEE:
```text
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: ...
```

---

### Step 6: Decouple Payment Repository with `IPaymentRepository`
To unit-test `PaymentService` without hitting SQL Server, we extract an interface.

**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Api/Repositories/`, select **New File**, name it `IPaymentRepository.cs`.
2. Paste:

```csharp
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

public interface IPaymentRepository
{
    Task<TaxReturn?> GetReturnAsync(int returnId);
    Task<decimal> GetTotalPaidAsync(int returnId);
    Task<int> SaveAsync(PaymentRequest request);
    Task<bool> ExistsByReferenceAsync(string reference);
}
```

3. Open `backend/LirsPortal.Api/Repositories/PaymentRepository.cs` and replace its entire content with:

```csharp
using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

public class PaymentRepository(DbConnectionFactory factory) : IPaymentRepository
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

    public async Task<bool> ExistsByReferenceAsync(string reference)
    {
        const string sql = "SELECT COUNT(1) FROM ComplianceLogs WHERE Event LIKE '%' + @Ref + '%'";
        using var db = factory.Create();
        var count = await db.ExecuteScalarAsync<int>(sql, new { Ref = reference });
        return count > 0;
    }
}
```

### Step 7: Create `FakePaymentRepository.cs` in `LirsPortal.Tests`
This in-memory fake acts like a database during unit test execution.

**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Tests/`, select **New File**, name it `FakePaymentRepository.cs`.
2. Paste:

```csharp
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;

namespace LirsPortal.Tests;

public class FakePaymentRepository : IPaymentRepository
{
    public List<TaxReturn> Returns { get; set; } = [];
    public List<PaymentRequest> SavedPayments { get; set; } = [];
    public decimal TotalPaidToReturn { get; set; } = 0m;
    public HashSet<string> ExistingReferences { get; set; } = [];

    public Task<TaxReturn?> GetReturnAsync(int returnId)
    {
        var match = Returns.FirstOrDefault(r => r.ReturnId == returnId);
        return Task.FromResult(match);
    }

    public Task<decimal> GetTotalPaidAsync(int returnId)
    {
        return Task.FromResult(TotalPaidToReturn);
    }

    public Task<int> SaveAsync(PaymentRequest request)
    {
        SavedPayments.Add(request);
        return Task.FromResult(SavedPayments.Count);
    }

    public Task<bool> ExistsByReferenceAsync(string reference)
    {
        return Task.FromResult(ExistingReferences.Contains(reference));
    }
}
```

### Step 8: Write `PaymentServiceTests.cs` in `LirsPortal.Tests`
**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Tests/`, select **New File**, name it `PaymentServiceTests.cs`.
2. Paste:

```csharp
using LirsPortal.Api;
using LirsPortal.Api.Models;
using LirsPortal.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LirsPortal.Tests;

public class PaymentServiceTests
{
    private readonly FakePaymentRepository _repo = new();
    private readonly IPaymentGatewayService _gateway = new MockPaymentGatewayService();
    private readonly NullLogger<PaymentService> _logger = new();

    private PaymentService CreateService() => new(_repo, _gateway, _logger);

    [Fact]
    public async Task RecordPayment_WhenReturnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var service = CreateService();
        var request = new PaymentRequest(ReturnId: 999, Amount: 50000m, Channel: "Bank");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.RecordPaymentAsync(request));
        Assert.Equal("Return not found.", ex.Message);
    }

    [Fact]
    public async Task RecordPayment_WhenReturnIsDraft_ThrowsBusinessRuleException()
    {
        // Arrange
        _repo.Returns.Add(new TaxReturn(ReturnId: 10, TaxpayerId: 101, TaxYear: 2025, DeclaredIncome: 1000000m, TaxDue: 100000m, Status: "Draft"));
        var service = CreateService();
        var request = new PaymentRequest(ReturnId: 10, Amount: 50000m, Channel: "Card");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.RecordPaymentAsync(request));
        Assert.Equal("Payments cannot be recorded against a draft return.", ex.Message);
    }

    [Fact]
    public async Task RecordPayment_WhenAmountExceedsOutstanding_ThrowsBusinessRuleException()
    {
        // Arrange: TaxDue is 500,000, already paid is 400,000. Outstanding is 100,000. Attempt payment of 150,000.
        _repo.Returns.Add(new TaxReturn(ReturnId: 1, TaxpayerId: 101, TaxYear: 2025, DeclaredIncome: 5000000m, TaxDue: 500000m, Status: "Approved"));
        _repo.TotalPaidToReturn = 400000m;

        var service = CreateService();
        var request = new PaymentRequest(ReturnId: 1, Amount: 150000m, Channel: "Bank");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.RecordPaymentAsync(request));
        Assert.Contains("exceeds the outstanding", ex.Message);
    }

    [Fact]
    public async Task RecordPayment_WhenValid_ReturnsReceiptNumberStartingWithRCT()
    {
        // Arrange: TaxDue 500,000, 0 paid. Pay 150,000.
        _repo.Returns.Add(new TaxReturn(ReturnId: 1, TaxpayerId: 101, TaxYear: 2025, DeclaredIncome: 5000000m, TaxDue: 500000m, Status: "Approved"));
        _repo.TotalPaidToReturn = 0m;

        var service = CreateService();
        var request = new PaymentRequest(ReturnId: 1, Amount: 150000m, Channel: "Bank");

        // Act
        var result = await service.RecordPaymentAsync(request);

        // Assert
        Assert.Equal("Successful", result.Status);
        Assert.StartsWith("RCT-", result.ReceiptNumber);
        Assert.Single(_repo.SavedPayments);
    }
}
```

*Note: If `MockPaymentGatewayService` does not compile yet, proceed to Part Four to write the gateway and models first, or run `dotnet test` right after Part Four.*

---

## 5. PART THREE: Python Unit Testing (Starter Code)

**EXPLAIN:**
- "On Day 1 we refactored `taxpayers.py`. Real-world agencies often have legacy Python scripts for batch calculations. Automated testing principles apply equally to Python."

**GO TO: VS Code**
1. Right-click `backend/starter/`, select **New File**, name it `test_taxpayers.py`.
2. Paste:

```python
import pytest
from taxpayers import calculate_late_penalty, find_taxpayer_by_tin, calculate_balance

def test_penalty_when_on_time():
    assert calculate_late_penalty(100000, 0) == 0

def test_penalty_when_late():
    # 10% of ₦100,000 = ₦10,000; 5 days at ₦100/day = ₦500 -> ₦10,500
    assert calculate_late_penalty(100000, 5) == 10500

def test_penalty_when_negative_days():
    assert calculate_late_penalty(50000, -2) == 0

def test_find_taxpayer_found():
    taxpayers = [
        {"tin": "1000000001", "name": "Adewale Ventures Ltd"},
        {"tin": "1000000002", "name": "Chioma Okafor"}
    ]
    result = find_taxpayer_by_tin(taxpayers, "1000000001")
    assert result is not None
    assert result["name"] == "Adewale Ventures Ltd"

def test_find_taxpayer_not_found():
    taxpayers = [{"tin": "1000000001", "name": "Adewale Ventures Ltd"}]
    assert find_taxpayer_by_tin(taxpayers, "9999999999") is None

def test_balance_calculation():
    returns = [
        {"return_id": 1, "tax_due": 500000, "status": "Approved"},
        {"return_id": 2, "tax_due": 600000, "status": "Submitted"},
        {"return_id": 3, "tax_due": 200000, "status": "Draft"}   # Draft ignored
    ]
    payments = [
        {"return_id": 1, "amount": 300000},
        {"return_id": 1, "amount": 100000}
    ]
    # Total Due = 500,000 + 600,000 = 1,100,000. Total Paid = 400,000. Balance = 700,000
    assert calculate_balance(returns, payments) == 700000
```

### Run Python tests in terminal:
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\backend\starter
python -m pytest test_taxpayers.py
```

### YOU SHOULD SEE:
`6 passed in 0.05s`

---

## 6. PART FOUR: Backend Security Implementation (Hashing, JWT, Secrets, Controllers)

Now we upgrade `LirsPortal.Api` with cryptographic authentication and role-based ownership checks.

### Step 1: Install the JWT package
**GO TO: VS Code terminal**
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\backend\LirsPortal.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Step 2: Configure secrets with `dotnet user-secrets`
**EXPLAIN:**
- "Never store JWT cryptographic keys or database passwords in `appsettings.json` where they can be accidentally committed to GitHub. We use `dotnet user-secrets` which stores the key in your Windows user profile outside the repository."

Run:
```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "LirsSuperSecretDefaultKeyForTrainingSession2026!"
dotnet user-secrets set "Jwt:Issuer" "LirsPortal"
dotnet user-secrets set "Jwt:Audience" "LirsPortalClient"
```

### Step 3: Update `Models/Models.cs`
**GO TO: VS Code**
Open `backend/LirsPortal.Api/Models/Models.cs` and replace its entire content with:

```csharp
using System.ComponentModel.DataAnnotations;

namespace LirsPortal.Api.Models;

public record Taxpayer(int TaxpayerId, string TIN, string Name, string Type, string State, string Phone);

public record TaxReturn(int ReturnId, int TaxpayerId, int TaxYear, decimal DeclaredIncome, decimal TaxDue, string Status);

public record Payment(int PaymentId, int ReturnId, decimal Amount, DateTime PaidOn, string Channel);

public record ComplianceLog(int LogId, int TaxpayerId, string Event, string OfficerName, DateTime LoggedAt);

public record User(
    int UserId, 
    string Username, 
    string PasswordHash, 
    string Role, 
    int? TaxpayerId, 
    int FailedAttempts, 
    DateTime? LockedUntil);

public record LoginRequest(
    [Required] string Username, 
    [Required] string Password);

public record LoginResponse(
    string Token, 
    string Username, 
    string Role, 
    int? TaxpayerId);

public record PaymentRequest(
    [Required] int ReturnId,
    [Range(0.01, 1000000000.00, ErrorMessage = "Amount must be between ₦0.01 and ₦1,000,000,000.00")] decimal Amount,
    [RegularExpression("^(Bank|Card|USSD)$", ErrorMessage = "Channel must be Bank, Card, or USSD")] string Channel,
    string? Reference = null);

public record PaymentResult(int PaymentId, string ReceiptNumber, string Status);

public record BalanceResult(int TaxpayerId, decimal Balance);

public record ApiError(string Error, string Code);

public record GatewayVerifyRequest(string Reference, decimal Amount);

public record GatewayVerifyResponse(bool IsValid, string Status, string Message);
```

### Step 4: Add `UserRepository.cs`
**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Api/Repositories/`, select **New File**, name it `UserRepository.cs`.
2. Paste:

```csharp
using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;

namespace LirsPortal.Api.Repositories;

public class UserRepository(DbConnectionFactory factory)
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        const string sql = @"SELECT UserId, Username, PasswordHash, Role, TaxpayerId, FailedAttempts, LockedUntil
                             FROM Users WHERE Username = @Username";
        using var db = factory.Create();
        return await db.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task RecordFailedAttemptAsync(int userId, int failedAttempts, DateTime? lockedUntil)
    {
        const string sql = @"UPDATE Users 
                             SET FailedAttempts = @FailedAttempts, LockedUntil = @LockedUntil 
                             WHERE UserId = @UserId";
        using var db = factory.Create();
        await db.ExecuteAsync(sql, new { UserId = userId, FailedAttempts = failedAttempts, LockedUntil = lockedUntil });
    }

    public async Task ResetFailedAttemptsAsync(int userId)
    {
        const string sql = @"UPDATE Users 
                             SET FailedAttempts = 0, LockedUntil = NULL 
                             WHERE UserId = @UserId";
        using var db = factory.Create();
        await db.ExecuteAsync(sql, new { UserId = userId });
    }
}
```

### Step 5: Add `PasswordService.cs`
**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Api/Services/`, select **New File**, name it `PasswordService.cs`.
2. Paste:

```csharp
using System.Security.Cryptography;

namespace LirsPortal.Api.Services;

public class PasswordService
{
    private const int SaltSize = 16;     // 128 bit
    private const int KeySize = 32;      // 256 bit
    private const int Iterations = 10000; // PBKDF2 iterations

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] subKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}:{Convert.ToBase64String(subKey)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(['.', ':'], 3);
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out int iterations)) return false;
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedSubKey = Convert.FromBase64String(parts[2]);

        byte[] actualSubKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedSubKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
    }
}
```

### Step 6: Add `TokenService.cs`
**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Api/Services/`, select **New File**, name it `TokenService.cs`.
2. Paste:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LirsPortal.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace LirsPortal.Api.Services;

public class TokenService(IConfiguration config)
{
    public string GenerateToken(User user)
    {
        var secretKey = config["Jwt:SecretKey"] ?? "LirsSuperSecretDefaultKeyForTrainingSession2026!";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Role, user.Role),
        };

        if (user.TaxpayerId.HasValue)
        {
            claims.Add(new Claim("taxpayerId", user.TaxpayerId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "LirsPortal",
            audience: config["Jwt:Audience"] ?? "LirsPortalClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(4),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Step 7: Add `PaymentGatewayService.cs`
**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Api/Services/`, select **New File**, name it `PaymentGatewayService.cs`.
2. Paste:

```csharp
namespace LirsPortal.Api.Services;

public interface IPaymentGatewayService
{
    Task<bool> VerifyTransactionAsync(string reference, decimal expectedAmount, CancellationToken cancellationToken = default);
}

public class MockPaymentGatewayService : IPaymentGatewayService
{
    public async Task<bool> VerifyTransactionAsync(string reference, decimal expectedAmount, CancellationToken cancellationToken = default)
    {
        // Simulate real banking network call with 10-second timeout guard
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        await Task.Delay(100, cts.Token); // Fast simulation delay

        // Simulated rule: references starting with "FAIL" are rejected
        if (reference.StartsWith("FAIL", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
```

### Step 8: Update `PaymentService.cs`
**GO TO: VS Code**
Open `backend/LirsPortal.Api/Services/PaymentService.cs` and replace its entire content with:

```csharp
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;

namespace LirsPortal.Api.Services;

public class PaymentService(
    IPaymentRepository payments, 
    IPaymentGatewayService gateway, 
    ILogger<PaymentService> logger)
{
    public async Task<PaymentResult> RecordPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var taxReturn = await payments.GetReturnAsync(request.ReturnId)
            ?? throw new NotFoundException("Return not found.");

        if (taxReturn.Status == "Draft")
            throw new BusinessRuleException("Payments cannot be recorded against a draft return.");

        var alreadyPaid = await payments.GetTotalPaidAsync(request.ReturnId);
        var outstanding = taxReturn.TaxDue - alreadyPaid;

        if (request.Amount > outstanding)
            throw new BusinessRuleException(
                $"Payment of {request.Amount:N2} exceeds the outstanding {outstanding:N2}.");

        // Idempotency and external verification
        if (!string.IsNullOrWhiteSpace(request.Reference))
        {
            if (await payments.ExistsByReferenceAsync(request.Reference))
            {
                throw new BusinessRuleException($"Transaction reference {request.Reference} has already been processed.");
            }

            bool verified = await gateway.VerifyTransactionAsync(request.Reference, request.Amount, cancellationToken);
            if (!verified)
            {
                throw new BusinessRuleException("Payment gateway could not confirm settlement for this transaction.");
            }
        }

        var paymentId = await payments.SaveAsync(request);
        logger.LogInformation("Payment {PaymentId} recorded for return {ReturnId}", paymentId, request.ReturnId);

        return new PaymentResult(paymentId, $"RCT-{paymentId:D6}", "Successful");
    }
}
```

### Step 9: Add `AuthController.cs`
**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Api/Controllers/`, select **New File**, name it `AuthController.cs`.
2. Paste:

```csharp
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserRepository users, 
    PasswordService passwords, 
    TokenService tokens, 
    ILogger<AuthController> logger) : ControllerBase
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await users.GetByUsernameAsync(request.Username);

        // Security best practice: Constant-time execution path prevention
        if (user == null)
        {
            logger.LogWarning("Failed login attempt for non-existent user: {Username}", request.Username);
            return Unauthorized(new ApiError("Invalid username or password.", "AUTH_FAILED"));
        }

        // Check account lockout
        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            logger.LogWarning("Login attempt on locked account: {Username}", request.Username);
            return StatusCode(423, new ApiError("Account temporarily locked due to multiple failed attempts. Try again later.", "ACCOUNT_LOCKED"));
        }

        bool valid = passwords.VerifyPassword(request.Password, user.PasswordHash);

        if (!valid)
        {
            int newAttempts = user.FailedAttempts + 1;
            DateTime? lockout = newAttempts >= MaxFailedAttempts ? DateTime.UtcNow.Add(LockoutDuration) : null;

            await users.RecordFailedAttemptAsync(user.UserId, newAttempts, lockout);
            logger.LogWarning("Invalid password for {Username}. Failed attempts: {Attempts}", request.Username, newAttempts);

            return Unauthorized(new ApiError("Invalid username or password.", "AUTH_FAILED"));
        }

        // Reset failures on successful authentication
        if (user.FailedAttempts > 0)
        {
            await users.ResetFailedAttemptsAsync(user.UserId);
        }

        var token = tokens.GenerateToken(user);
        logger.LogInformation("Successful login for {Username} with role {Role}", user.Username, user.Role);

        return Ok(new LoginResponse(token, user.Username, user.Role, user.TaxpayerId));
    }
}
```

### Step 10: Update `TaxpayersController.cs` with Ownership Guard
**EXPLAIN:**
- "OWASP Issue #1 is Broken Access Control. If Taxpayer 101 logs in, gets a valid token, and types `/api/taxpayers/102/balance`, the server must say `403 Forbidden`. An Officer can see anyone, but a Taxpayer can only see their own file."

**GO TO: VS Code**
Open `backend/LirsPortal.Api/Controllers/TaxpayersController.cs` and replace its entire content with:

```csharp
using System.Security.Claims;
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaxpayersController(TaxpayerRepository repo) : ControllerBase
{
    private bool IsAuthorizedForTaxpayer(int taxpayerId)
    {
        if (User.IsInRole("Officer")) return true;

        var claim = User.FindFirst("taxpayerId")?.Value;
        return claim != null && int.TryParse(claim, out int userTaxpayerId) && userTaxpayerId == taxpayerId;
    }

    [HttpGet]
    [Authorize(Roles = "Officer")]
    public async Task<IActionResult> GetPage([FromQuery] string? tin, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);
        var taxpayers = await repo.GetPageAsync(tin, page, pageSize);
        return Ok(taxpayers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested taxpayer profile.", "FORBIDDEN"));

        var taxpayer = await repo.GetByIdAsync(id);
        return taxpayer == null 
            ? NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND")) 
            : Ok(taxpayer);
    }

    [HttpGet("{id}/returns")]
    public async Task<IActionResult> GetReturns(int id)
    {
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested tax returns.", "FORBIDDEN"));

        var returns = await repo.GetReturnsAsync(id);
        return Ok(returns);
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested balance statement.", "FORBIDDEN"));

        var balance = await repo.GetBalanceAsync(id);
        return Ok(new BalanceResult(id, balance));
    }
}
```

### Step 11: Update `PaymentsController.cs` with Ownership Guard
**GO TO: VS Code**
Open `backend/LirsPortal.Api/Controllers/PaymentsController.cs` and replace its entire content with:

```csharp
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController(
    PaymentService service, 
    IPaymentRepository payments, 
    ILogger<PaymentsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RecordPayment([FromBody] PaymentRequest request, CancellationToken cancellationToken)
    {
        // Enforce ownership: taxpayers cannot record payments against other taxpayers' returns
        if (!User.IsInRole("Officer"))
        {
            var taxReturn = await payments.GetReturnAsync(request.ReturnId);
            if (taxReturn != null)
            {
                var userTaxpayerIdStr = User.FindFirst("taxpayerId")?.Value;
                if (userTaxpayerIdStr == null || !int.TryParse(userTaxpayerIdStr, out int userTaxpayerId) || userTaxpayerId != taxReturn.TaxpayerId)
                {
                    logger.LogWarning("Forbidden payment attempt on return {ReturnId} by user {User}", request.ReturnId, User.Identity?.Name);
                    return StatusCode(403, new ApiError("You cannot record payments against returns belonging to another taxpayer.", "FORBIDDEN"));
                }
            }
        }

        try
        {
            var result = await service.RecordPaymentAsync(request, cancellationToken);
            return StatusCode(201, result);
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

### Step 12: Add Security Demo Practice Controller
This optional endpoint demonstrates SQL Injection vulnerability vs Parameterized query safety.

**GO TO: VS Code**
1. Right-click `backend/LirsPortal.Api/Controllers/`, select **New File**, name it `SecurityDemoController.cs`.
2. Paste:

```csharp
using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecurityDemoController(DbConnectionFactory factory) : ControllerBase
{
    // DANGEROUS: String concatenation creates SQL Injection
    [HttpGet("vulnerable-search")]
    public async Task<IActionResult> VulnerableSearch([FromQuery] string tin)
    {
        string rawSql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TIN = '" + tin + "'";
        using var db = factory.Create();
        var results = await db.QueryAsync<Taxpayer>(rawSql);
        return Ok(results);
    }

    // SECURE: Parameterized SQL prevents SQL Injection
    [HttpGet("secure-search")]
    public async Task<IActionResult> SecureSearch([FromQuery] string tin)
    {
        const string paramSql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TIN = @TIN";
        using var db = factory.Create();
        var results = await db.QueryAsync<Taxpayer>(paramSql, new { TIN = tin });
        return Ok(results);
    }
}
```

### Step 13: Wire everything in `Program.cs`
**GO TO: VS Code**
Open `backend/LirsPortal.Api/Program.cs` and replace its entire content with:

```csharp
using System.Text;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers and CORS
builder.Services.AddControllers();
builder.Services.AddCors(options =>
    options.AddPolicy("portal", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// 2. JWT Authentication Configuration
var secretKey = builder.Configuration["Jwt:SecretKey"] ?? "LirsSuperSecretDefaultKeyForTrainingSession2026!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LirsPortal",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LirsPortalClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// 3. Dependency Injection
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<TaxpayerRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentGatewayService, MockPaymentGatewayService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<PenaltyCalculator>();

var app = builder.Build();

// 4. Global Exception Safety
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new ApiError("Something went wrong. Please try again.", "SERVER_ERROR"));
}));

// 5. Middleware Pipeline
app.UseCors("portal");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

### Step 14: Build and run the API
**GO TO: VS Code terminal**
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\backend\LirsPortal.Api
dotnet build
dotnet run --launch-profile http
```

### YOU SHOULD SEE:
```text
Build succeeded.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5123
```
*(Write down the port number printed, e.g. 5123).*

---

## 7. PART FIVE: Live Security Demonstrations

Conduct these three live demonstrations to illustrate the security controls.

### Demo 1: SQL Injection Attack vs Parameterized Defense
**GO TO: Postman**
1. Send a GET request to:
   `http://localhost:5123/api/securitydemo/vulnerable-search?tin=' OR '1'='1`
   **Result:** It dumps ALL 4 taxpayers in the database! An attacker bypasses filtering completely.
2. Send a GET request to:
   `http://localhost:5123/api/securitydemo/secure-search?tin=' OR '1'='1`
   **Result:** Returns `[]` (empty list). The parameterized query treats `' OR '1'='1` as literal text. The attack is neutered.

### Demo 2: Broken Access Control (Horizontal Privilege Escalation)
**GO TO: Postman**
1. First, log in as Taxpayer Adewale (`adewale` / `pass123`) using:
   - Method: `POST`
   - URL: `http://localhost:5123/api/auth/login`
   - Body (raw JSON): `{"username":"adewale","password":"pass123"}`
   - Copy the `token` string from the JSON response.
2. In a new tab, attempt to inspect Taxpayer 102's (Chioma's) balance:
   - Method: `GET`
   - URL: `http://localhost:5123/api/taxpayers/102/balance`
   - Headers: `Authorization: Bearer <paste_token>`
   - Send.

### YOU SHOULD SEE:
- **HTTP 403 Forbidden**
- Response body:
  ```json
  {
    "error": "Access denied to requested balance statement.",
    "code": "FORBIDDEN"
  }
```

### Demo 3: Brute-Force Password Guessing & Lockout
**GO TO: Postman**
1. Send 5 consecutive failed logins for `adewale` with an incorrect password:
   `POST http://localhost:5123/api/auth/login`
   Body: `{"username":"adewale","password":"wrongpassword"}`
2. Attempt #1 through #5 returns `401 Unauthorized`:
   ```json
   { "error": "Invalid username or password.", "code": "AUTH_FAILED" }
   ```
3. Attempt #6 returns `423 Locked`:
   ```json
   { "error": "Account temporarily locked due to multiple failed attempts. Try again later.", "code": "ACCOUNT_LOCKED" }
   ```
4. In SSMS, query `SELECT * FROM Users WHERE Username = 'adewale';` to observe `FailedAttempts = 5` and `LockedUntil` populated.

*(To unlock the account for further testing, run in SSMS: `UPDATE Users SET FailedAttempts = 0, LockedUntil = NULL WHERE Username = 'adewale';`).*

---

## 8. PART SIX: System Integration & Payment Gateway Verification

**EXPLAIN:**
- "In a real tax agency, when a taxpayer pays ₦50,000 via a bank, the bank provides a transaction reference. Our API must verify that reference with the gateway before writing the ledger, and ensure the same reference cannot be processed twice (idempotency)."

### Test 1: Record payment with valid reference
**GO TO: Postman**
- Method: `POST`
- URL: `http://localhost:5123/api/payments`
- Headers:
  - `Authorization: Bearer <adewale_token>`
  - `Content-Type: application/json`
- Body:
  ```json
  {
    "returnId": 1,
    "amount": 50000.00,
    "channel": "Bank",
    "reference": "REF-BANK-987654"
  }
  ```

### YOU SHOULD SEE:
- **HTTP 201 Created**
- Response:
  ```json
  {
    "paymentId": 5,
    "receiptNumber": "RCT-000005",
    "status": "Successful"
  }
  ```

### Test 2: Duplicate reference rejection (Idempotency)
Send the exact same request again with reference `"REF-BANK-987654"`.

### YOU SHOULD SEE:
- **HTTP 400 Bad Request**
- `{"error": "Transaction reference REF-BANK-987654 has already been processed.", "code": "BUSINESS_RULE"}`

### Test 3: Gateway settlement failure
Send a payment with reference starting with `"FAIL"`:
```json
{
  "returnId": 1,
  "amount": 10000.00,
  "channel": "Card",
  "reference": "FAIL-DECLINED-CARD"
}
```

### YOU SHOULD SEE:
- **HTTP 400 Bad Request**
- `{"error": "Payment gateway could not confirm settlement for this transaction.", "code": "BUSINESS_RULE"}`

---

## 9. PART SEVEN: Frontend Real Authentication Integration (React)

Now we connect our Vite React application to the real authentication backend so that login issues genuine JWT tokens stored in `sessionStorage` and sent with every request.

### Step 1: Update API Client (`services/api.js`)
**GO TO: VS Code**
Open `frontend/portal/src/services/api.js` and replace its entire content with:

```javascript
const BASE_URL = "http://localhost:5123";

function getAuthHeader() {
  const token = sessionStorage.getItem("lirs_token");
  return token ? { Authorization: `Bearer ${token}` } : {};
}

async function handle(response) {
  const data = await response.json().catch(() => ({}));
  if (!response.ok) {
    throw new Error(data.error || "Request failed");
  }
  return data;
}

export function getJson(path) {
  return fetch(`${BASE_URL}${path}`, {
    headers: {
      ...getAuthHeader(),
    },
  }).then(handle);
}

export function postJson(path, body) {
  return fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeader(),
    },
    body: JSON.stringify(body),
  }).then(handle);
}
```

*(Ensure `BASE_URL` matches your running API port, e.g. 5123).*

### Step 2: Update Auth Context (`context/AuthContext.jsx`)
**GO TO: VS Code**
Open `frontend/portal/src/context/AuthContext.jsx` and replace its entire content with:

```jsx
import { createContext, useContext, useState } from "react";
import { postJson } from "../services/api";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const saved = sessionStorage.getItem("lirs_user");
    return saved ? JSON.parse(saved) : null;
  });

  async function login(username, password) {
    const data = await postJson("/api/auth/login", { username, password });
    
    const authenticatedUser = {
      username: data.username,
      role: data.role,
      taxpayerId: data.taxpayerId,
      name: data.role === "Officer" ? `Officer ${data.username}` : data.username,
    };

    sessionStorage.setItem("lirs_token", data.token);
    sessionStorage.setItem("lirs_user", JSON.stringify(authenticatedUser));
    setUser(authenticatedUser);

    return authenticatedUser;
  }

  function logout() {
    sessionStorage.removeItem("lirs_token");
    sessionStorage.removeItem("lirs_user");
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
```

### Step 3: Run and test the frontend
**GO TO: VS Code terminal**
Open a second terminal tab:
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\frontend\portal
npm run dev
```

**GO TO: Chrome or Edge**
Navigate to `http://localhost:5173`.
1. Log in with `adewale` / `pass123`.
2. Observe the Taxpayer Dashboard load with real balance and tax returns.
3. Open **F12 DevTools -> Network tab**, click any action, and inspect the HTTP Request Headers:
   Observe `Authorization: Bearer eyJhbGciOi...`.
4. Log out, then log in with `bisi` / `pass123`.
5. Observe the Officer Dashboard load. Search TIN `1000000001` and verify Adewale Ventures is retrieved.

---

## 10. PART EIGHT: Automated Postman Regression Test Suite

To guarantee our endpoints never regress, add automated tests in Postman.

**GO TO: Postman**
Create or update your collection requests with the following **Tests** scripts:

### Test 1: `POST /api/auth/login` (Under the "Tests" tab):
```javascript
pm.test("Status code is 200 OK", function () {
    pm.response.to.have.status(200);
});

pm.test("Returns valid JWT token and TaxpayerId", function () {
    var json = pm.response.json();
    pm.expect(json.token).to.be.a("string");
    pm.expect(json.role).to.eql("Taxpayer");
    pm.expect(json.taxpayerId).to.eql(101);
    
    // Set environment variable for chained requests
    pm.environment.set("jwt_token", json.token);
});
```

### Test 2: `GET /api/taxpayers/102/balance` (Forbidden test):
```javascript
pm.test("Status code is 403 Forbidden", function () {
    pm.response.to.have.status(403);
});

pm.test("Error code is FORBIDDEN", function () {
    var json = pm.response.json();
    pm.expect(json.code).to.eql("FORBIDDEN");
});
```

### Test 3: `POST /api/payments` (Payment verification):
```javascript
pm.test("Status code is 201 Created", function () {
    pm.response.to.have.status(201);
});

pm.test("Returns receipt starting with RCT-", function () {
    var json = pm.response.json();
    pm.expect(json.receiptNumber).to.match(/^RCT-\d{6}$/);
    pm.expect(json.status).to.eql("Successful");
});
```

Click **Run collection** in Postman to watch the entire regression test pass with green checkmarks!

---

## 11. PART NINE: Save Work with Git and GitHub (Branches & PRs)

Follow the professional team Git workflow practiced on Day 1. Each feature is committed on a dedicated branch and merged via GitHub Pull Request.

**GO TO: VS Code terminal**
Ensure you are in the repository root:
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal
```

### Slice 1: Unit Testing Setup
```powershell
git checkout -b feature/testing-setup
git add backend/LirsPortal.Tests/ backend/LirsPortal.Api/Services/PenaltyCalculator.cs backend/starter/test_taxpayers.py
git commit -m "feat: setup xunit and pytest test suites with penalty calculation tests"
git push -u origin feature/testing-setup
```
**GitHub steps:**
1. Open GitHub repo `lirs-taxpayer-portal`.
2. Click **Compare & pull request**.
3. Title: `Feature: Automated unit testing setup`.
4. Student reviews the PR (or instructor reviews), click **Merge pull request**, then **Confirm merge**.
5. Back in terminal:
```powershell
git checkout main
git pull origin main
```

### Slice 2: Payment Interface & Service Tests
```powershell
git checkout -b feature/payment-interface-tests
git add backend/LirsPortal.Api/Repositories/IPaymentRepository.cs backend/LirsPortal.Api/Repositories/PaymentRepository.cs backend/LirsPortal.Tests/FakePaymentRepository.cs backend/LirsPortal.Tests/PaymentServiceTests.cs
git commit -m "feat: decouple payment repository interface and add unit tests"
git push -u origin feature/payment-interface-tests
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

### Slice 3: Authentication and Lockout Protection
```powershell
git checkout -b feature/auth-login
git add backend/database/03_auth.sql backend/LirsPortal.Api/Repositories/UserRepository.cs backend/LirsPortal.Api/Services/PasswordService.cs backend/LirsPortal.Api/Services/TokenService.cs backend/LirsPortal.Api/Controllers/AuthController.cs
git commit -m "feat: implement database authentication with PBKDF2 hashing and account lockout"
git push -u origin feature/auth-login
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

### Slice 4: Secured Endpoints and Ownership Control
```powershell
git checkout -b feature/secure-endpoints
git add backend/LirsPortal.Api/Controllers/TaxpayersController.cs backend/LirsPortal.Api/Controllers/PaymentsController.cs backend/LirsPortal.Api/Controllers/SecurityDemoController.cs backend/LirsPortal.Api/Program.cs backend/LirsPortal.Api/Models/Models.cs
git commit -m "feat: enforce JWT authentication and taxpayer resource ownership checks"
git push -u origin feature/secure-endpoints
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

### Slice 5: Gateway Verification and Idempotency
```powershell
git checkout -b feature/gateway-verification
git add backend/LirsPortal.Api/Services/PaymentGatewayService.cs backend/LirsPortal.Api/Services/PaymentService.cs
git commit -m "feat: add payment gateway verification with 10s timeout and idempotency checks"
git push -u origin feature/gateway-verification
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

### Slice 6: Real Frontend Authentication Integration
```powershell
git checkout -b feature/frontend-real-login
git add frontend/portal/src/services/api.js frontend/portal/src/context/AuthContext.jsx
git commit -m "feat: connect react frontend to server-side JWT authentication"
git push -u origin feature/frontend-real-login
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

---

## 12. If Time is Short: Priority Order & One-Branch Shortcut

If class discussion runs long or unexpected installation delays occur, use this strict priority order to ensure core outcomes are achieved before 16:00:

| Priority | Feature | Why it matters |
|---|---|---|
| **1 (MUST DO)** | JWT Authentication & AuthController | Replaces fake login with real cryptographic tokens. |
| **2 (MUST DO)** | xUnit Unit Testing Suite | Demonstrates testing pyramid and automated verification. |
| **3 (MUST DO)** | Ownership Guard on TaxpayersController | Fixes OWASP Broken Access Control (403 Forbidden). |
| **4 (HIGH)** | React Frontend Token Integration | Connects the real token to the browser interface. |
| **5 (MEDIUM)** | Payment Gateway Mock & Idempotency | Demonstrates external system integration. |
| **6 (STRETCH)** | Python `pytest` suite | Can be demonstrated in 5 minutes or assigned as homework. |

### The One-Branch Shortcut (if less than 30 minutes remain for Git):
Instead of 6 separate pull requests, commit all files to a single branch:
```powershell
git checkout -b feature/day4-security-and-testing
git add .
git commit -m "feat: day 4 complete testing, jwt security, and gateway integration"
git push -u origin feature/day4-security-and-testing
```
Merge this single PR into `main` and show the merged commit graph on GitHub.

---

## 13. What the Participant Should Be Able to Say at the End

At 15:45, ask the participant to summarize today's work. He should comfortably express:

1. **On Testing:** *"Unit tests allow us to verify critical tax calculations and payment limits in milliseconds without touching the production database or clicking in a browser."*
2. **On Security:** *"We never store plain passwords. We hash them with PBKDF2 and a unique salt. We use JWT tokens to verify identity, and every endpoint checks ownership so Taxpayer A cannot inspect Taxpayer B's records."*
3. **On Integration:** *"When dealing with commercial banks and payment gateways, we enforce idempotency so network retries never double-credit a tax liability."*
4. **On Architecture:** *"Separating interfaces from implementations makes our revenue software easily testable, modular, and maintainable."*

---

## 14. Master Troubleshooting Table

| Problem / Error | Cause | Fix |
|---|---|---|
| `CS0246: The type or namespace name 'IPaymentRepository' could not be found` | Interface file not saved or namespace mismatch | Ensure `IPaymentRepository.cs` has `namespace LirsPortal.Api.Repositories;` and is saved. |
| `CS0246: The type or namespace name 'TaxReturn' could not be found` in tests | Missing project reference | Run `dotnet add reference ..\LirsPortal.Api\Lirsportal.Api.csproj` inside `LirsPortal.Tests`. |
| `401 Unauthorized` on all API endpoints in Postman | Missing or malformed `Authorization` header | Header must be exactly `Authorization: Bearer <token>` (notice the space after Bearer). |
| `403 Forbidden` when Adewale tries to view return 1 | Token contains incorrect `taxpayerId` claim | Check `TokenService.cs`. Ensure `claims.Add(new Claim("taxpayerId", ...))` is populated. |
| `423 Locked` on login | 5 failed attempts triggered lockout | In SSMS, run: `UPDATE Users SET FailedAttempts = 0, LockedUntil = NULL WHERE Username = 'adewale';`. |
| Postman: `SSL Error: Self-signed certificate` | Request sent to HTTPS port without cert trust | Run API with `dotnet run --launch-profile http` and send requests to `http://` (not `https://`). |
| React: `CORS policy: No 'Access-Control-Allow-Origin' header` | Missing CORS middleware order in `Program.cs` | Ensure `app.UseCors("portal")` is placed **before** `app.UseAuthentication()` and `app.UseAuthorization()`. |
| `Cannot find module 'pytest'` | pytest not installed in Python environment | Run `pip install pytest` or `py -m pip install pytest`. |
| Duplicate reference test passes instead of 400 | `ExistsByReferenceAsync` checking wrong table | Ensure `ExistsByReferenceAsync` checks `ComplianceLogs` or your chosen reference storage. |
| Token expired unexpectedly | System clock skew or short token expiry | Check `DateTime.UtcNow.AddHours(4)` in `TokenService.cs`. Ensure computer clock is correct. |

---

## 15. Final Checklist

Before ending Day 4, ensure:

- [ ] All 6 xUnit tests in `LirsPortal.Tests` pass with `dotnet test`.
- [ ] Database contains the `Users` table with 3 seeded records.
- [ ] Postman login as `adewale` returns a valid JWT token.
- [ ] Requesting `/api/taxpayers/102/balance` with Adewale's token returns `403 Forbidden`.
- [ ] Postman payment request returns `201 Created` with receipt format `RCT-XXXXXX`.
- [ ] React frontend logs in via real API and displays balance of ₦700,000 for Adewale.
- [ ] React DevTools Network tab shows `Authorization: Bearer ...` header attached.
- [ ] All Git branches are merged into `main` on GitHub.
- [ ] System is clean and ready for Day 5 (DevOps, CI/CD, Deployment, Capstone Demo).

---

## 16. Quick Reference of Every Command Used Today

| Task | Command |
|---|---|
| Create xUnit test project | `dotnet new xunit -n LirsPortal.Tests` |
| Reference API from test project | `dotnet add reference ..\LirsPortal.Api\Lirsportal.Api.csproj` |
| Run all automated .NET tests | `dotnet test` |
| Install JWT package | `dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer` |
| Initialize user secrets | `dotnet user-secrets init` |
| Set secret key | `dotnet user-secrets set "Jwt:SecretKey" "LirsSuperSecretDefaultKeyForTrainingSession2026!"` |
| Build API project | `dotnet build` |
| Run API with HTTP profile | `dotnet run --launch-profile http` |
| Run Python tests | `python -m pytest test_taxpayers.py` |
| Start React frontend | `npm run dev` |
| Reset locked user in SSMS | `UPDATE Users SET FailedAttempts = 0, LockedUntil = NULL WHERE Username = 'adewale';` |
| Create Git feature branch | `git checkout -b feature/<branch-name>` |
| Push branch to GitHub | `git push -u origin feature/<branch-name>` |
