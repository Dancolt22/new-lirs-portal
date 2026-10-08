# LIRS TAXPAYER MINI-PORTAL: MASTER ORAL DEFENSE PLAYBOOK
**System:** Lagos State Internal Revenue Service (LIRS) Mini-Portal  
**Candidate Role:** Lead Software Engineer & System Architect  
**Curriculum Alignment:** NIIT Fortesoft / LIRS Professional Software Development (April 2026)  
**Evaluation Target:** Final Capstone Defense & Project Presentation (Day 5)

---

## 1. YOUR DEFENSE MINDSET & PERSONA

When you stand before the evaluators, instructors, and LIRS directors on Friday, **you are not a student reciting steps—you are the Lead System Architect defending architectural, business, and security decisions.**

### The 3 Core Pillars of Your Defense:
1. **Public Sector Context:** "Every line of code was written with the realities of Lagos State revenue collection in mind—high transaction concurrency, strict auditability, zero data leakage, and statutory compliance (NDPR)."
2. **Deliberate Engineering Choices:** "We did not choose our stack by accident. We chose ASP.NET Core for enterprise type-safety, Dapper for micro-ORM throughput, SQL Server for ACID monetary guarantees, and React for responsive citizen self-service."
3. **Defense in Depth:** "Security is enforced at every layer: input sanitization in React, CORS origin shielding at the API gateway, 4-stage business rule validation in C#, parameterized SQL in Dapper, and table constraints in SQL Server."

---

## 2. HIGH-LEVEL SYSTEM ARCHITECTURE DEFENSE

```
[ CITIZEN & OFFICER CLIENTS ]
  Google Chrome / Microsoft Edge (React 18 + Vite SPA)
  Port: http://localhost:5173
        │
        ▼  [Encrypted HTTP JSON Requests]
[ API GATEWAY & APPLICATION LAYER ]
  ASP.NET Core Web API (.NET 8/10 on Kestrel Server)
  Port: http://localhost:5123
  ├── Security: CORS Policy ("portal"), Centralized Error Masking Middleware
  ├── Routing & Controllers: TaxpayersController, PaymentsController
  ├── Domain Logic: PaymentService (4 Strict Statutory Business Rules)
  └── Data Access: Dapper Micro-ORM + Microsoft.Data.SqlClient
        │
        ▼  [TCP / Port 1433 - Parameterized T-SQL]
[ ENTERPRISE STORAGE LAYER ]
  Microsoft SQL Server Express (.\SQLEXPRESS)
  Database: LirsPortal
  ├── Tables: Taxpayers, TaxReturns, Payments, AuditLogs
  └── Integrity: Primary/Foreign Keys, Identity Counters, DECIMAL(18,2)
```

### Anticipated Panel Question: *"Why this multi-tier architecture instead of a monolithic single app?"*
> **Your Winning Answer:**  
> *"Separating the frontend from the backend guarantees separation of concerns. In a government agency like LIRS, user interfaces evolve rapidly—today it is a React web portal, tomorrow a mobile app or USSD gateway. By decoupling the core business logic and database behind a secure RESTful API, any authorized client can consume the same verified tax calculation and payment engine without duplicating backend code or risking database corruption."*

---

## 3. DAY-BY-DAY DEFENSE BREAKDOWN

### DAY 1: Engineering Fundamentals, Agile & Version Control
**Panel Question:** *"How did your team structure the project before writing code?"*
- **Requirements Engineering:** Translated LIRS executive needs into formal User Stories (US-01 through US-07 in `docs/requirements.md`) with measurable acceptance criteria.
- **Agile/Scrum in Government:** Adopted iterative sprints instead of rigid Waterfall. This allowed rapid delivery of working increments (Day 2 backend, Day 3 frontend) while accommodating evolving tax regulations.
- **Git Feature Branching Workflow:**
  - `main` branch represents deployable production code.
  - Work was partitioned into feature branches (e.g., `feature/cors`, `feature/US-04-taxpayer-dashboard`).
  - Merged via GitHub Pull Requests after peer code review.
  - Ignored build artifacts (`bin/`, `obj/`, `node_modules/`, `.env`) via `.gitignore` to prevent repository bloat and credential exposure.

---

### DAY 2: Database Design, Micro-ORM & RESTful API
**Panel Question 1:** *"Why did you use Dapper instead of Entity Framework Core?"*
> **Your Winning Answer:**  
> *"LIRS processes millions of high-frequency transactions during tax filing seasons. While Entity Framework Core is a feature-rich full ORM, it introduces heavy change-tracking overhead and can generate inefficient SQL joins. Dapper is a lightweight micro-ORM that gives our engineering team 100% control over raw T-SQL queries. It compiles query mappers directly to IL bytecode, providing near-native ADO.NET execution speeds while completely eliminating SQL injection risks through parameterized queries."*

**Panel Question 2:** *"Why use `DECIMAL(18,2)` instead of `FLOAT` or `DOUBLE` for monetary amounts?"*
> **Your Winning Answer:**  
> *"In financial and revenue systems, using `FLOAT` or `DOUBLE` is a critical accounting violation. Floating-point numbers use binary approximation (IEEE 754), which introduces minute rounding errors (such as 0.1 + 0.2 yielding 0.30000000000000004). Over millions of taxpayer transactions, those fractions accumulate into millions of Naira in discrepancies. `DECIMAL(18,2)` uses exact base-10 representation, guaranteeing statutory precision down to the exact kobo."*

**Panel Question 3:** *"Walk us through your Payment Business Logic."*
> **Your Winning Answer:**  
> *"In `PaymentService.cs`, we enforce a 4-stage validation pipeline before any payment record touches the database:
> 1. **Rule 1 (Statutory Floor):** Payment amount must be strictly greater than ₦0.00.
> 2. **Rule 2 (Entity Existence):** Target taxpayer account must exist in the database.
> 3. **Rule 3 (Assessment Status):** Returns in 'Draft' status cannot be paid; only 'Submitted' or 'Approved' returns are payable.
> 4. **Rule 4 (Overpayment Shield):** Payment cannot exceed the remaining unpaid balance (`TaxDue - TotalPaid`). If a taxpayer owes ₦600,000, attempting to pay ₦650,000 is immediately rejected with a custom `OverpaymentException`."*

**Panel Question 4:** *"How do you handle API errors and exceptions?"*
> **Your Winning Answer:**  
> *"We implemented centralized exception handling in `Program.cs`. We never let raw C# database stack traces leak to the client—that would violate OWASP security principles by revealing server internals to potential attackers. Instead, domain exceptions are trapped and returned as structured, sanitized JSON (`ApiError` record with clean messages and error codes like `NOT_FOUND` or `OVERPAYMENT`)."*

---

### DAY 3: React Frontend Architecture & User Experience (UX)
**Panel Question 1:** *"Why React and Vite for a government portal?"*
> **Your Winning Answer:**  
> *"Citizens expect fast, responsive interactions without frustrating page reloads. With Vite, our build and Hot Module Replacement are lightning-fast. In React, our UI is componentized: reusable components like `<Naira />` guarantee that every financial figure across the portal adheres to the official `en-NG` currency format (`₦700,000.00`). `<TopBar />` unifies branding and session state across all pages."*

**Panel Question 2:** *"How does state management work when a taxpayer makes a payment?"*
> **Your Winning Answer:**  
> *"We use reactive state in `TaxpayerDashboard.jsx`. When the citizen clicks 'Submit Payment', `handlePay` fires `postJson('/api/payments', ...)`. Upon receiving HTTP 201 Created from the API, we increment a `reload` state counter. Because `reload` is in the dependency array of our `useEffect` hook (`[user.taxpayerId, reload]`), React automatically triggers a background GET request to re-fetch the updated balance. The balance card drops on screen instantly without any manual page reload."*

**Panel Question 3:** *"How do you handle role-based navigation and security in the frontend?"*
> **Your Winning Answer:**  
> *"We use `AuthContext` to broadcast user identity across the application tree, and `<ProtectedRoute role='Taxpayer'>` as a client-side route guard. If an unauthenticated user attempts to visit a dashboard, they are redirected to `/login`. If a Taxpayer attempts to access `/officer`, the guard halts rendering and displays an access-denied card.  
> **Crucial Engineering Caveat:** Front-end route guards enhance user experience and prevent accidental navigation. Real security must always reside on the server via API authentication and database permissions."*

---

### DAY 4: Testing, Security & Regulatory Compliance (NDPR / OWASP)
**Panel Question 1:** *"How does this project comply with the Nigeria Data Protection Regulation (NDPR)?"*
> **Your Winning Answer:**  
> *"NDPR mandates strict confidentiality, purpose limitation, and accountability for personal data:
> 1. **Data Minimization & Role Separation:** Citizens only have access to their own taxpayer ID and returns. Only authorized revenue officers can access the TIN search directory.
> 2. **Audit Trails:** Every sensitive transaction (payments, assessments) records timestamps, user IDs, and channels in our relational schema to guarantee non-repudiation.
> 3. **Confidentiality:** Passwords will never be stored in plain text (Day 4 introduces cryptographic hashing via BCrypt/PBKDF2), and communications travel over TLS-encrypted HTTPS."*

**Panel Question 2:** *"How does your architecture address the OWASP Top 10 vulnerabilities?"*
> **Your Winning Answer:**  
> - **A01: Broken Access Control:** Enforced server-side in API controllers through role checks and taxpayer ID ownership validation.
> - **A02: Cryptographic Failures:** Sensitive credentials protected via industry-standard hashing; JWT access tokens signed with HMAC-SHA256 secrets.
> - **A03: Injection (SQLi):** 100% neutralized by Dapper's parameterized query engine. Dynamic SQL string concatenation is strictly banned in our coding standards.
> - **A05: Security Misconfiguration:** Strict CORS origin lockdown (`WithOrigins("http://localhost:5173")`), disabling unnecessary HTTP methods and wildcard headers in production.
> - **A09: Security Logging & Monitoring:** All runtime errors and transaction events logged without storing sensitive PII or credentials in log streams.

---

### DAY 5: CI/CD, DevOps & Enterprise Scalability
**Panel Question 1:** *"How would you deploy and maintain this system in LIRS production infrastructure?"*
> **Your Winning Answer:**  
> *"We adopt modern DevOps practices:
> 1. **Containerization:** The ASP.NET Core API and React static build are packaged into lightweight Docker containers for environment parity across staging and production.
> 2. **CI/CD Automation:** Automated GitHub Actions pipeline that triggers on pull requests to `main`—running `dotnet test` and `npm run build` to catch regressions before deployment.
> 3. **Scalability & High Concurrency:** The API is stateless, meaning multiple instances of Kestrel can run behind a load balancer (such as NGINX or Azure Application Gateway). Read-heavy queries (like TIN lookups) can be cached in Redis to protect the SQL Server primary instance during peak filing deadlines."*

---

## 4. RAPID-FIRE DEFENSE CHEAT SHEET

| When They Ask About... | Mention These Exact Keywords |
|---|---|
| **Database Choice** | ACID compliance, `DECIMAL(18,2)` kobo precision, Foreign key referential integrity. |
| **Dapper Micro-ORM** | Microsecond mapping speed, raw SQL optimization, parameterized injection immunity. |
| **API Architecture** | ASP.NET Core Kestrel, REST semantics (HTTP 200/201/400/404), Centralized error masking. |
| **CORS Policy** | Same-Origin Policy defense, explicit origin whitelisting (`localhost:5173`), preflight `OPTIONS` validation. |
| **React Architecture** | Single Page Application, Virtual DOM reconciliation, Context API global state, hooks (`useState`, `useEffect`). |
| **Security & Compliance** | OWASP Top 10 mitigation, NDPR compliance, Role-Based Access Control (RBAC), Auditability. |
| **Git & Quality** | Feature branches, conventional commits, Pull Request code reviews, automated CI verification. |

---

## 5. CLOSING STATEMENT FOR YOUR DEFENSE

*"In conclusion, this LIRS Taxpayer Mini-Portal is not merely a prototype—it is an enterprise-grade architectural blueprint. By bridging modern frontend reactivity with a hardened, highly performant ASP.NET Core backend and an ACID-compliant database, we have delivered a system that empowers Lagos State taxpayers with seamless self-service while safeguarding state revenue and data integrity."*
