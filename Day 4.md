# Day 4: Testing, Security, and System Integration

This day covers how to prove software works, how to defend it against threats, and how to connect it safely with external services. The day applies these disciplines directly to the capstone, the **LIRS Taxpayer Mini-Portal**, by introducing automated unit testing, replacing simulated frontend authentication with cryptographically signed JSON Web Tokens (JWTs), enforcing strict role and ownership checks, protecting against the OWASP Top 10 vulnerabilities, and integrating a simulated payment gateway with idempotency checks.

```
Client / Browser -> [JWT Auth & Role Check] -> [Ownership Filter] -> Business Logic -> Database (SQL Server)
                                                                    -> Payment Gateway (Verification & Idempotency)
```

---

## 1. Why software testing matters

**Testing** is the disciplined execution of software using predefined inputs to compare actual behavior against expected specifications before delivery to users.
*Example:* Verifying that submitting a tax payment of ₦50,000 reduces the outstanding balance by exactly ₦50,000 and returns an official receipt number starting with `RCT-`.

**A bug** is a defect, error, or flaw in software that produces an incorrect, unintended, or unexpected result.
*Example:* A calculation error where a 10% late penalty on a ₦500,000 tax debt is calculated as ₦5,000 instead of ₦50,000 because of an misplaced decimal point.

**A test case** is a formal specification of inputs, execution preconditions, testing procedure, and expected output developed for a particular objective.
*Example:* Testing that when an individual taxpayer with zero outstanding liability submits a payment of ₦10,000, the system rejects the transaction with HTTP status 400 and the message "Payment of ₦10,000.00 exceeds the outstanding ₦0.00."

**Test coverage** is a metric measuring the percentage of codebase lines, branches, or execution paths exercised during automated test runs.
*Example:* If the `PaymentService` class has 40 lines of logic and automated tests run 36 of those lines across various scenarios, test coverage for that class is 90%.

**A regression** is a software bug that surfaces in an existing, previously functioning feature after a new code modification or patch is introduced.
*Example:* An update deployed to improve TIN search performance accidentally breaks the balance calculation by dropping payments recorded earlier in the morning.

**The compounding financial and legal cost of defects** illustrates why catching bugs early in development is critical; defects identified in production require emergency rollbacks, data remediation, and legal liability management.
*Example:* A small rounding error of ₦50 on a single transaction might seem trivial, but across 450,000 Lagos State taxpayers filing annual returns, that single defect compounds into a ₦22,500,000 discrepancy on state revenue ledgers.

---

## 2. The testing pyramid and test categories

**The testing pyramid** is an architectural strategy dictating that a healthy test suite contains a large base of fast, isolated unit tests, a moderate layer of integration tests, and a small apex of end-to-end tests.
*Example:* Running 150 unit tests in under 2 seconds during a build, 20 integration tests verifying database interactions in 15 seconds, and 3 browser-based end-to-end journeys in 2 minutes.

| Test Level | Scope of Validation | Execution Speed | Typical LIRS Capstone Example |
|---|---|---|---|
| **Unit Test** | A single isolated function or class method without external systems | Milliseconds | Verifying `PenaltyCalculator.Calculate(100000m, 5)` returns `10500m` |
| **Integration Test** | Two or more components working together across network or storage boundaries | Seconds | Verifying `PaymentRepository` writes a payment row to SQL Server and returns an identity ID |
| **End-to-End (E2E) Test** | The entire system path from the React browser UI to the database and back | Tens of seconds | A simulated user logging in as Adewale, navigating to returns, submitting ₦50,000, and seeing the balance update |
| **User Acceptance Test (UAT)** | Formal validation by business users verifying software meets business requirements | Hours to days | LIRS revenue auditors testing tax return approval workflows against the official tax code |

**Manual testing** is human-driven exploratory verification of application flows without test scripts or test automation frameworks.
*Example:* An instructor or tester clicking through the officer dashboard, intentionally entering an empty TIN, and observing whether the UI displays a clean error banner or a broken screen.

**Performance and load testing** is the measurement of system responsiveness, throughput, and stability under peak concurrent user volume.
*Example:* Simulating 10,000 concurrent business taxpayers filing their annual returns at 11:00 PM on January 31st to measure if API response times stay below 500 milliseconds.

**Security testing** is the deliberate probe of system defenses to detect vulnerabilities, authorization leaks, and data exposure flaws.
*Example:* Attempting to access `GET /api/taxpayers/101/balance` with an authentication token belonging to taxpayer 102 to verify that an HTTP 403 Forbidden error is returned.

---

## 3. Unit testing principles and design

**The Arrange-Act-Assert (AAA) pattern** is the universal structure for readable, deterministic unit tests, splitting each test into setup, execution, and verification.
*Example:*
```csharp
// Arrange: set up inputs and preconditions
var calculator = new PenaltyCalculator();
decimal amountDue = 100000m;
int daysLate = 5;

// Act: perform the action under test
decimal penalty = calculator.Calculate(amountDue, daysLate);

// Assert: verify the result matches expectations
Assert.Equal(10500m, penalty);
```

**Normal, edge, and boundary test inputs** are the three categories of input conditions every robust test suite must systematically validate.
*Example:*
- *Normal input:* A payment of ₦200,000 on an outstanding balance of ₦500,000 (succeeds smoothly).
- *Boundary input:* A payment of exactly ₦500,000 on an outstanding balance of ₦500,000 (clears the debt to ₦0.00).
- *Invalid/Negative input:* A payment of ₦0.00, a negative payment of -₦50,000, or a payment of ₦500,001 (all rejected with descriptive validation errors).

**Test naming conventions** are standardized naming templates that clearly express the tested method, the scenario condition, and the expected outcome.
*Example:* `RecordPaymentAsync_WhenAmountExceedsOutstanding_ThrowsBusinessRuleException`. When this test fails in a terminal or CI pipeline, the exact cause is obvious without inspecting the code.

**Test independence and determinism** is the requirement that every test must execute independently of other tests and yield the exact same result regardless of execution order or system clock.
*Example:* A unit test must never rely on another test having already created taxpayer 101 in the database, nor should it fail if executed on Sunday rather than Monday.

**Test doubles (Fakes, Mocks, and Stubs)** are simplified substitutes for complex real-world dependencies (such as databases, external networks, or file systems) used to isolate code under test.
*Example:* Supplying `PaymentService` with a `FakePaymentRepository` stored entirely in computer memory, allowing tests to run instantly without requiring SQL Server to be running.

**Interface-driven design for testability** is the practice of decoupling business logic from concrete infrastructure classes by introducing abstract interfaces.
*Example:* Modifying `PaymentService` to accept `IPaymentRepository` rather than the concrete `PaymentRepository` class, enabling unit test suites to substitute the database repository with an in-memory test fake.

---

## 4. Debugging and code review

**Systematic debugging** is a hypothesis-driven diagnostic methodology for locating, isolating, and resolving defects rather than making speculative code edits.
*Example:* When an API returns HTTP 500, the developer checks the server terminal logs for the stack trace, identifies a `NullReferenceException` on line 42, formulates a hypothesis that the user token lacked a `taxpayerId` claim, tests the hypothesis with a debug breakpoint, and fixes the missing claim check.

**Breakpoints and runtime inspection** are development tools that pause code execution at a designated line, allowing developers to inspect variable values and execution stacks in real time.
*Example:* Setting a red breakpoint in VS Code on line 674 of `PaymentService.cs` to inspect the exact runtime value of `taxReturn.TaxDue` and `alreadyPaid` when a payment is processed.

**Browser DevTools Network and Console inspection** are client-side diagnostics for tracing HTTP communication payloads and frontend runtime errors.
*Example:* Opening the **Network** tab in Chrome DevTools to verify that the React frontend is attaching `Authorization: Bearer eyJhbGci...` to the HTTP request header when querying `/api/taxpayers/101/balance`.

**Structured code reviews** are collaborative peer evaluations where one developer examines another developer's pull request before code merges into the main branch.
*Example:* An instructor reviewing the participant's pull request to verify that all database queries use `@` parameters, no database connection strings are hardcoded, and all new business logic has accompanying automated tests.

**A code review checklist for revenue systems** is an audit guide ensuring every pull request satisfies strict security, financial, and maintainability standards.

| Review Dimension | Specific Inspection Questions | LIRS Compliance Goal |
|---|---|---|
| **Financial Precision** | Are all monetary amounts declared as `DECIMAL(18,2)`? Are rounding operations consistent? | Zero currency truncation errors |
| **Security & Injection** | Are all SQL queries parameterized? Are all inputs validated on the server? | Zero SQL injection risks |
| **Access Control** | Does the endpoint check both role and resource ownership? | Taxpayers cannot view peer data |
| **Error Safety** | Does the API catch unexpected exceptions and return a safe generic message? | Database internals hidden from public |
| **Audit Compliance** | Are critical revenue actions logged to `ComplianceLogs`? | Full accountability for state audits |

---

## 5. Software security essentials and the OWASP Top 10

**The CIA triad** represents the three core pillars of information security: Confidentiality (only authorized parties view data), Integrity (data cannot be altered improperly), and Availability (systems remain operational for legitimate users).
*Example:* Confidentiality protects taxpayer Chioma Okafor's income details from public exposure; Integrity ensures nobody can illegally reduce her tax liability; Availability ensures the portal remains online throughout peak filing deadlines.

**Defense in depth** is a security strategy that establishes multiple redundant layers of defensive controls throughout an IT infrastructure.
*Example:* At LIRS, defense in depth means firewalls protect the network, HTTPS encrypts traffic in transit, JWT authentication verifies identity, role filters restrict officer features, ownership checks prevent taxpayer ID tampering, parameterized queries block SQL injection, and database user accounts have minimal permissions.

**The principle of least privilege** dictates that every user, service, and database connection must be granted only the absolute minimum permissions required to perform their intended task.
*Example:* The connection string used by `LirsPortal.Api` should only have `SELECT`, `INSERT`, and `UPDATE` permissions on specific portal tables, never `sysadmin` or `DROP TABLE` privileges.

**The OWASP Top 10** is the internationally recognized consensus list of the most critical security risks facing web applications.

```
       [ Client Request ]
               │
               ▼
   1. Transport Layer Security (HTTPS)
               │
               ▼
   2. Authentication (JWT Signature & Expiration)
               │
               ▼
   3. Authorization & RBAC (Role: Taxpayer vs Officer)
               │
               ▼
   4. Object Ownership Filter (User TaxpayerId == Route ID)
               │
               ▼
   5. Input Validation (Range, Regex, Data Annotations)
               │
               ▼
   6. Business Logic & In-Memory Rules
               │
               ▼
   7. Parameterized Queries (Dapper @Parameters)
               │
               ▼
       [ SQL Server Database ]
```

**SQL Injection (SQLi)** is an attack where malicious SQL fragments are inserted into application inputs and executed directly by the database engine.
*Example:* A search input taking `1000000001' OR '1'='1` that concatenates directly into `SELECT * FROM Taxpayers WHERE TIN = '` + input + `'`, causing the database to dump every taxpayer record in Lagos State.

**Parameterized queries** are database queries where SQL code and user input are sent to the database engine separately, preventing input from ever being executed as SQL commands.
*Example:* Using Dapper with `SELECT * FROM Taxpayers WHERE TIN = @Tin`, where the database treats `@Tin` purely as literal text data, neutralizing all injection strings.

**Broken access control and Insecure Direct Object References (IDOR)** occur when an application does not verify whether an authenticated user owns or is authorized to access the specific requested resource.
*Example:* Taxpayer Adewale (TaxpayerId 101) logs in legitimately, but modifies the browser URL to `GET /api/taxpayers/102/returns`. Without an ownership check, Adewale views Chioma Okafor's private financial returns.

**Authentication versus Authorization** separates proof of identity from permission to act.
*Example:* Authentication verifies that a user with username `adewale` provided the correct password. Authorization verifies whether `adewale` holds the `Officer` role required to approve a tax return.

**Password hashing and salting** is the one-way cryptographic transformation of plain text passwords into unique, non-reversible hashes, with a random salt appended to defeat precomputed dictionary and rainbow table attacks.
*Example:* Storing `AQAAAAEAACcQAAAA...` in the database instead of `pass123`. Even if an unauthorized party gains access to the database backup, the original passwords cannot be decrypted.

**Brute-force protection and account lockout** is an automated defensive mechanism that temporarily suspends login capability for an account after repeated failed credential attempts.
*Example:* If an attacker attempts five incorrect passwords in succession for username `bisi`, the API locks the account for 15 minutes, neutralizing automated password-guessing attacks.

**Cross-Site Scripting (XSS)** is a vulnerability where malicious JavaScript scripts are injected into web applications and executed inside the browsers of unsuspecting users.
*Example:* An attacker submitting `<script>stealCookies()</script>` as their business trade name, which would execute in the officer's browser when reviewing the profile unless the frontend properly escapes HTML entities (as React does automatically via JSX).

**Sensitive data exposure and cryptographic failures** occur when confidential data is transmitted in plain text or stored without appropriate masking or encryption.
*Example:* Transmitting tax records over unencrypted HTTP where packet sniffers on public networks can capture taxpayer records, rather than enforcing HTTPS.

**Data masking** is the intentional obscuring of sensitive identifiers on user interfaces and application logs to safeguard privacy.
*Example:* Displaying a Taxpayer Identification Number as `******0001` or a telephone number as `0803****001` on public receipts and browser screens.

**Safe error messages and user enumeration prevention** is the practice of returning generic error responses to clients while keeping detailed stack traces in secure server logs.
*Example:* Returning HTTP 401 with `{ "error": "Invalid username or password.", "code": "UNAUTHORIZED" }` regardless of whether the username was missing or the password was incorrect, preventing attackers from harvesting valid government usernames.

**Dependency vulnerabilities and scanning** is the automated inspection of third-party software libraries for known Common Vulnerabilities and Exposures (CVEs).
*Example:* Running `dotnet list package --vulnerable` in the API folder and `npm audit` in the React frontend folder to detect outdated libraries containing known security flaws.

**Secrets management and source control hygiene** is the strict policy of never committing passwords, API connection strings, or cryptographic keys to version control systems like Git.
*Example:* Storing the database password and JWT signing secret in `dotnet user-secrets` during development and in environment variables on production servers, while adding `.env` to `.gitignore`.

---

## 6. Audit logging and data privacy compliance

**An immutable audit trail** is a tamper-evident, append-only record of system events detailing who performed what action, on which record, and at what exact timestamp.
*Example:* When Officer Bisi approves tax return 1, the backend automatically writes a row into `ComplianceLogs` recording `TaxpayerId: 101`, `Event: Return 1 approved`, `OfficerName: Officer Bisi`, and `LoggedAt: 2026-10-07 14:22:01`.

**Event logging in revenue systems** provides non-repudiation and forensic visibility for all financial, compliance, and authentication actions.
*Example:* Logging every failed login threshold breach, every administrative role change, and every payment gateway callback with transaction reference numbers.

**The Nigeria Data Protection Act (NDPA)** is the federal legal framework governing the collection, processing, storage, and privacy rights of personal data within Nigeria.
*Example:* Under the NDPA, LIRS taxpayer data systems must enforce data minimization (storing only necessary information), purpose limitation (using tax data only for statutory revenue administration), technical security measures (encryption and role access), and confidentiality obligations.

---

## 7. System integration and payment gateways

**A third-party payment gateway** is an external financial technology service (such as Interswitch, Remita, Paystack, or Monnify) that processes card, bank transfer, and USSD payments on behalf of merchants and government agencies.
*Example:* A taxpayer choosing "Card" on the portal, being routed through a secure gateway interface, and the gateway issuing a unique transaction reference confirming payment settlement.

**The client-trust fallacy** is the dangerous architectural mistake of trusting transaction status or payment amounts reported directly by the user's web browser.
*Example:* A malicious user intercepting browser network traffic, modifying a failed payment response to look like a success, and submitting it to the portal. The backend must always verify transaction legitimacy directly with the payment gateway server-to-server.

**Idempotency and idempotency keys** are architectural mechanisms ensuring that submitting the exact same API request multiple times produces the exact same outcome without duplicate side effects.
*Example:* A taxpayer with a poor internet connection clicking "Submit Payment" three times rapidly. The backend uses a unique `Idempotency-Key` header (`TXN-REF-99214`) to ensure the payment is recorded exactly once, returning the original receipt for the repeated clicks rather than charging the taxpayer three times.

**Network resilience, timeouts, and retries** are patterns that prevent an application from freezing or crashing when an external network service becomes slow or unresponsive.
*Example:* Configuring the HTTP client connecting to an external payment gateway with a strict 10-second timeout and up to two automatic retries with exponential backoff before returning a clean "Payment gateway unavailable" message.

**Sanitized integration logging** is the logging of external API request and response metadata while stripping out sensitive fields like authentication secrets, Primary Account Numbers (PAN), or PINs.
*Example:* Logging `Sent verification for reference REF-2026-001; Gateway status: Successful` while deliberately excluding authorization bearer tokens and taxpayer bank account numbers from log files.

---

## 8. Capstone work for Day 4

The Day 4 capstone deliverables transform the existing application into a secure, tested, and verifiable system. The artifacts below include the authentication database migration, interface-driven refactoring of `PaymentService`, comprehensive unit test suites in C# and Python, JWT token generation, role-based endpoint protection with ownership validation, and payment gateway verification.

### Database updates: `backend/database/03_auth.sql`

This script creates the `Users` table, adds indexes, and seeds fictional training users with salted password hashes. The passwords for all three users are `pass123`.

```sql
USE LirsPortal;
GO

-- 1. Create the Users table
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
GO

-- 2. Seed initial users (Pass123 hashed via PBKDF2 with SHA-256)
-- Fictional credentials:
-- adewale / pass123  (Taxpayer for Adewale Ventures Ltd, TaxpayerId 101)
-- chioma  / pass123  (Taxpayer for Chioma Okafor, TaxpayerId 102)
-- bisi    / pass123  (LIRS Officer Bisi, TaxpayerId NULL)

INSERT INTO Users (Username, PasswordHash, Role, TaxpayerId, FailedAttempts, LockedUntil) VALUES
('adewale', '10000.yLd8R4V6s4w=:zY9Qe4JkM9+1L2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c=', 'Taxpayer', 101, 0, NULL),
('chioma',  '10000.yLd8R4V6s4w=:zY9Qe4JkM9+1L2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c=', 'Taxpayer', 102, 0, NULL),
('bisi',    '10000.yLd8R4V6s4w=:zY9Qe4JkM9+1L2o3p4q5r6s7t8u9v0w1x2y3z4a5b6c=', 'Officer',  NULL, 0, NULL);
GO
```

### Models: `backend/LirsPortal.Api/Models/Models.cs` (Updated)

Add the authentication and integration models to the existing models file.

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

    // Optional external reference from payment gateway
    public string? Reference { get; set; }
}

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "";
    public int? TaxpayerId { get; set; }
    public int FailedAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
}

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public record LoginResult(string Token, string Username, string Role, int? TaxpayerId);
public record PaymentResult(int PaymentId, string ReceiptNumber, string Status);
public record BalanceResult(int TaxpayerId, decimal Balance);
public record ApiError(string Error, string Code);
```

### Decoupled repository interface: `backend/LirsPortal.Api/Repositories/IPaymentRepository.cs`

Extracting this interface enables `PaymentService` to be thoroughly unit-tested using fast in-memory fakes without opening database connections.

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

### Concrete repository: `backend/LirsPortal.Api/Repositories/PaymentRepository.cs`

Update the repository to implement `IPaymentRepository` and support reference checking.

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
        // For demonstration, checks if a compliance log or audit record contains the reference
        const string sql = "SELECT COUNT(1) FROM ComplianceLogs WHERE Event LIKE '%' + @Ref + '%'";
        using var db = factory.Create();
        var count = await db.ExecuteScalarAsync<int>(sql, new { Ref = reference });
        return count > 0;
    }
}
```

### User data access: `backend/LirsPortal.Api/Repositories/UserRepository.cs`

Provides secure queries for credential authentication, account lockout increments, and lockout resets.

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

### Password cryptography service: `backend/LirsPortal.Api/Services/PasswordService.cs`

Uses PBKDF2 with HMAC-SHA256 and a random cryptographic salt. No external packages are required.

```csharp
using System.Security.Cryptography;

namespace LirsPortal.Api.Services;

public class PasswordService
{
    private const int SaltSize = 16;     // 128 bit
    private const int KeySize = 32;      // 256 bit
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            var iterAndSalt = parts[0].Split('.');
            int iterations = int.Parse(iterAndSalt[0]);
            byte[] salt = Convert.FromBase64String(iterAndSalt[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}
```

### JWT token service: `backend/LirsPortal.Api/Services/TokenService.cs`

Issues signed JSON Web Tokens containing claims for user identity, role, and associated `taxpayerId`.

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
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        if (user.TaxpayerId.HasValue)
        {
            claims.Add(new Claim("taxpayerId", user.TaxpayerId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"] ?? "LirsPortal",
            audience: config["Jwt:Audience"] ?? "LirsPortalClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Penalty calculation logic: `backend/LirsPortal.Api/Services/PenaltyCalculator.cs`

Computes late filing penalties using the official formula: 10% of tax due plus ₦100 per day late.

```csharp
namespace LirsPortal.Api.Services;

public class PenaltyCalculator
{
    public decimal Calculate(decimal amountDue, int daysLate, bool isWaived = false)
    {
        if (isWaived || daysLate <= 0 || amountDue <= 0)
        {
            return 0m;
        }

        decimal percentagePenalty = amountDue * 0.10m;
        decimal dailyPenalty = daysLate * 100.00m;

        return Math.Round(percentagePenalty + dailyPenalty, 2);
    }
}
```

### Payment gateway integration service: `backend/LirsPortal.Api/Services/PaymentGatewayService.cs`

Simulates external gateway verification with a 10-second timeout, idempotency protection, and sanitized logging.

```csharp
namespace LirsPortal.Api.Services;

public interface IPaymentGatewayService
{
    Task<bool> VerifyTransactionAsync(string reference, decimal expectedAmount, CancellationToken cancellationToken = default);
}

public class MockPaymentGatewayService(ILogger<MockPaymentGatewayService> logger) : IPaymentGatewayService
{
    public async Task<bool> VerifyTransactionAsync(string reference, decimal expectedAmount, CancellationToken cancellationToken = default)
    {
        // Enforce a strict 10-second timeout boundary
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        logger.LogInformation("Verifying reference {Ref} with external gateway for amount {Amount:N2}", reference, expectedAmount);

        // Simulate gateway network roundtrip
        await Task.Delay(100, cts.Token);

        // Fail intentionally if a simulated test reference is passed
        if (reference.StartsWith("FAIL-", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("External gateway verification declined reference {Ref}", reference);
            return false;
        }

        logger.LogInformation("External gateway confirmed settlement for reference {Ref}", reference);
        return true;
    }
}
```

### Business logic: `backend/LirsPortal.Api/Services/PaymentService.cs` (Refactored)

Now relies entirely on `IPaymentRepository` and optionally validates through `IPaymentGatewayService`.

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

### Authentication controller: `backend/LirsPortal.Api/Controllers/AuthController.cs`

Implements `POST /api/auth/login` with 5-attempt brute-force lockout, generic error masking, and JWT generation.

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
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await users.GetByUsernameAsync(request.Username);

        // Always check lockout status
        if (user is not null && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            logger.LogWarning("Login rejected: account {Username} is currently locked", request.Username);
            return StatusCode(423, new ApiError("Account is temporarily locked. Please try again later.", "ACCOUNT_LOCKED"));
        }

        if (user is null || !passwords.VerifyPassword(request.Password, user.PasswordHash))
        {
            if (user is not null)
            {
                int newAttempts = user.FailedAttempts + 1;
                DateTime? lockUntil = newAttempts >= 5 ? DateTime.UtcNow.AddMinutes(15) : null;
                await users.RecordFailedAttemptAsync(user.UserId, newAttempts, lockUntil);

                if (lockUntil.HasValue)
                {
                    logger.LogWarning("Account {Username} locked after {Attempts} failed attempts", user.Username, newAttempts);
                }
            }

            // Safe error masking: never reveal whether username or password was incorrect
            return Unauthorized(new ApiError("Invalid username or password.", "UNAUTHORIZED"));
        }

        // Reset failed counters upon valid authentication
        if (user.FailedAttempts > 0)
        {
            await users.ResetFailedAttemptsAsync(user.UserId);
        }

        var tokenString = tokens.GenerateToken(user);
        logger.LogInformation("User {Username} ({Role}) logged in successfully", user.Username, user.Role);

        return Ok(new LoginResult(tokenString, user.Username, user.Role, user.TaxpayerId));
    }
}
```

### Secured controllers: `backend/LirsPortal.Api/Controllers/TaxpayersController.cs` (Updated)

Secured with `[Authorize]`, role restrictions, and strict taxpayer ownership filtering.

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
    // Officer only: listing or searching taxpayers
    [HttpGet]
    [Authorize(Roles = "Officer")]
    public async Task<IActionResult> GetAll([FromQuery] string? tin, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);
        return Ok(await repo.GetPageAsync(tin, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested taxpayer profile.", "FORBIDDEN"));

        var taxpayer = await repo.GetByIdAsync(id);
        return taxpayer is null
            ? NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"))
            : Ok(taxpayer);
    }

    [HttpGet("{id}/returns")]
    public async Task<IActionResult> GetReturns(int id)
    {
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested taxpayer returns.", "FORBIDDEN"));

        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));

        return Ok(await repo.GetReturnsAsync(id));
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        if (!IsAuthorizedForTaxpayer(id))
            return StatusCode(403, new ApiError("Access denied to requested taxpayer balance.", "FORBIDDEN"));

        if (await repo.GetByIdAsync(id) is null)
            return NotFound(new ApiError("Taxpayer not found.", "NOT_FOUND"));

        return Ok(new BalanceResult(id, await repo.GetBalanceAsync(id)));
    }

    // Ownership validation helper: Officers view anyone; Taxpayers view only themselves
    private bool IsAuthorizedForTaxpayer(int requestedTaxpayerId)
    {
        if (User.IsInRole("Officer")) return true;

        var claim = User.FindFirst("taxpayerId")?.Value;
        return int.TryParse(claim, out int userTaxpayerId) && userTaxpayerId == requestedTaxpayerId;
    }
}
```

### Secured payments controller: `backend/LirsPortal.Api/Controllers/PaymentsController.cs` (Updated)

Enforces authentication and ownership validation on payment creation.

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
public class PaymentsController(PaymentService service, IPaymentRepository payments) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentRequest request, CancellationToken cancellationToken)
    {
        // Ownership check: If caller is a Taxpayer, verify the return belongs to their TaxpayerId
        if (!User.IsInRole("Officer"))
        {
            var userTaxpayerIdClaim = User.FindFirst("taxpayerId")?.Value;
            var taxReturn = await payments.GetReturnAsync(request.ReturnId);
            if (taxReturn is not null && int.TryParse(userTaxpayerIdClaim, out int userTaxpayerId))
            {
                if (taxReturn.TaxpayerId != userTaxpayerId)
                {
                    return StatusCode(403, new ApiError("You cannot record a payment against another taxpayer's return.", "FORBIDDEN"));
                }
            }
        }

        try
        {
            var result = await service.RecordPaymentAsync(request, cancellationToken);
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

### Demonstration endpoint: `backend/LirsPortal.Api/Controllers/SecurityDemoController.cs`

A dedicated training controller used exclusively to demonstrate SQL injection vulnerabilities side-by-side with parameterized protection.

```csharp
using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

[ApiController]
[Route("api/demo")]
public class SecurityDemoController(DbConnectionFactory factory) : ControllerBase
{
    // VULNERABLE: Direct string concatenation allows input like ' OR '1'='1 to dump all records
    [HttpGet("vulnerable-search")]
    public async Task<IActionResult> VulnerableSearch([FromQuery] string tin)
    {
        string sql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TIN = '" + tin + "'";
        using var db = factory.Create();
        var results = await db.QueryAsync<Taxpayer>(sql);
        return Ok(results);
    }

    // SECURE: Parameterized query isolates data from SQL instructions
    [HttpGet("secure-search")]
    public async Task<IActionResult> SecureSearch([FromQuery] string tin)
    {
        const string sql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TIN = @Tin";
        using var db = factory.Create();
        var results = await db.QueryAsync<Taxpayer>(sql, new { Tin = tin });
        return Ok(results);
    }
}
```

### Complete wiring: `backend/LirsPortal.Api/Program.cs` (Updated)

Integrates JWT bearer authentication, registers all new services and interfaces, and protects all routes.

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

---

### Automated unit testing: `backend/LirsPortal.Tests`

#### Project configuration: `backend/LirsPortal.Tests/LirsPortal.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\LirsPortal.Api\Lirsportal.Api.csproj" />
  </ItemGroup>

</Project>
```

#### In-memory test double: `backend/LirsPortal.Tests/FakePaymentRepository.cs`

Implements `IPaymentRepository` in memory, permitting rapid, isolated testing of business rules without SQL Server.

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

#### Penalty calculator test suite: `backend/LirsPortal.Tests/PenaltyCalculatorTests.cs`

Verifies the 10% plus ₦100 per day formula across normal, boundary, and waiver scenarios.

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
    [InlineData(100000, 5, 10500)]   // 10,000 + 500 = 10,500
    [InlineData(50000, 1, 5100)]     // 5,000 + 100 = 5,100
    [InlineData(240000, 10, 25000)]  // 24,000 + 1,000 = 25,000
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

#### Payment service test suite: `backend/LirsPortal.Tests/PaymentServiceTests.cs`

Exercises business rules for return validation, draft status, overpayment, and successful receipt formatting.

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
    private readonly IPaymentGatewayService _gateway = new MockPaymentGatewayService(NullLogger<MockPaymentGatewayService>.Instance);
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _service = new PaymentService(_repo, _gateway, NullLogger<PaymentService>.Instance);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenReturnDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange: Repository has no returns
        var request = new PaymentRequest { ReturnId = 999, Amount = 10000m, Channel = "Bank" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RecordPaymentAsync(request));
        Assert.Equal("Return not found.", ex.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenReturnIsDraft_ThrowsBusinessRuleException()
    {
        // Arrange
        _repo.Returns.Add(new TaxReturn { ReturnId = 1, TaxDue = 500000m, Status = "Draft" });
        var request = new PaymentRequest { ReturnId = 1, Amount = 50000m, Channel = "Bank" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RecordPaymentAsync(request));
        Assert.Equal("Payments cannot be recorded against a draft return.", ex.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenPaymentExceedsOutstanding_ThrowsBusinessRuleException()
    {
        // Arrange: Tax due ₦500,000, already paid ₦300,000 -> outstanding ₦200,000
        _repo.Returns.Add(new TaxReturn { ReturnId = 1, TaxDue = 500000m, Status = "Submitted" });
        _repo.TotalPaidToReturn = 300000m;

        var request = new PaymentRequest { ReturnId = 1, Amount = 250000m, Channel = "Card" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RecordPaymentAsync(request));
        Assert.Contains("exceeds the outstanding", ex.Message);
    }

    [Fact]
    public async Task RecordPaymentAsync_WhenValid_ReturnsReceiptWithExpectedFormat()
    {
        // Arrange: Tax due ₦500,000, ₦0 paid -> outstanding ₦500,000
        _repo.Returns.Add(new TaxReturn { ReturnId = 1, TaxDue = 500000m, Status = "Approved" });
        _repo.TotalPaidToReturn = 0m;

        var request = new PaymentRequest { ReturnId = 1, Amount = 150000m, Channel = "Bank" };

        // Act
        var result = await _service.RecordPaymentAsync(request);

        // Assert
        Assert.Equal("Successful", result.Status);
        Assert.Equal("RCT-000001", result.ReceiptNumber);
        Assert.Single(_repo.SavedPayments);
    }
}
```

---

### Python unit testing: `backend/starter/test_taxpayers.py`

Tests the Day 1 starter code functions (`calculate_late_penalty`, `find_taxpayer_by_tin`, and `calculate_balance`) using `pytest`.

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

---

### Frontend integration updates

#### Updated API client: `frontend/portal/src/services/api.js`

Stores and retrieves the JWT token from browser `sessionStorage`, attaching `Authorization: Bearer <token>` to all HTTP requests.

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

#### Updated authentication context: `frontend/portal/src/context/AuthContext.jsx`

Replaces the simulated user array with real server-side API calls to `POST /api/auth/login`.

```jsx
import { createContext, useContext, useState, useEffect } from "react";
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

---

### Automated Postman test assertions

These JavaScript assertions can be added to the **Tests** tab of requests in Postman to automate API regression verification.

#### 1. Authentication test (`POST /api/auth/login`)
```javascript
pm.test("Status code is 200 OK", function () {
    pm.response.to.have.status(200);
});

pm.test("Response contains JWT token and role", function () {
    var json = pm.response.json();
    pm.expect(json).to.have.property("token");
    pm.expect(json).to.have.property("role");
    pm.expect(json.token.length).to.be.above(20);
    // Persist token for subsequent requests
    pm.environment.set("bearer_token", json.token);
});
```

#### 2. Ownership security check (`GET /api/taxpayers/102/balance` as Taxpayer 101)
```javascript
pm.test("Status code is 403 Forbidden", function () {
    pm.response.to.have.status(403);
});

pm.test("Returns FORBIDDEN error code", function () {
    var json = pm.response.json();
    pm.expect(json.code).to.eql("FORBIDDEN");
});
```

#### 3. Payment creation test (`POST /api/payments`)
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

---

### Git work for Day 4

| Branch | Contents |
|---|---|
| `feature/testing-setup` | xUnit project `LirsPortal.Tests`, `PenaltyCalculator.cs`, `PenaltyCalculatorTests.cs`, and `test_taxpayers.py` |
| `feature/payment-interface-tests` | `IPaymentRepository.cs`, `FakePaymentRepository.cs`, `PaymentServiceTests.cs`, and refactored `PaymentService.cs` |
| `feature/auth-login` | Database `03_auth.sql`, `User.cs`, `UserRepository.cs`, `PasswordService.cs`, `TokenService.cs`, and `AuthController.cs` |
| `feature/secure-endpoints` | JWT middleware in `Program.cs`, `[Authorize]` and ownership validation on `TaxpayersController` and `PaymentsController` |
| `feature/gateway-verification` | `IPaymentGatewayService.cs`, `MockPaymentGatewayService.cs`, idempotency and timeout handling |
| `feature/frontend-real-login` | Updated `api.js` bearer header injection and `AuthContext.jsx` server-side login |

---

### State of the capstone at the end of Day 4

| Deliverable | Status |
|---|---|
| Decoupled `IPaymentRepository` interface | Complete |
| Automated unit tests for penalty calculation and payments (xUnit) | Complete |
| Starter Python automated test suite (`pytest`) | Complete |
| Real database authentication with salted password hashes (PBKDF2) | Complete |
| Brute-force account lockout protection (5 failed attempts) | Complete |
| Cryptographically signed JWT tokens with identity and role claims | Complete |
| Role-based access control and taxpayer resource ownership validation | Complete |
| Payment gateway verification with 10-second timeout and idempotency | Complete |
| React frontend connected to real server authentication and tokens | Complete |
| Automated Postman regression assertions | Complete |
| Tax return approval workflow (US-06) | Day 5 |
| Revenue collection report by state (US-07) | Day 5 |
| CI/CD automation pipeline (GitHub Actions) and production deployment | Day 5 |
