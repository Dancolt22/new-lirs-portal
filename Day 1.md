# Day 1: Software Development Fundamentals and Best Practices

This day covers how professional software is planned, organized, written, documented, and shared by a team. Every concept is illustrated with a LIRS example, and the day's skills are applied to the capstone: the **LIRS Taxpayer Mini-Portal**.

## The capstone project

The LIRS Taxpayer Mini-Portal is a small, fictional version of the systems LIRS runs. A **taxpayer** logs in, sees their balance, views their returns, and records a payment. A **LIRS officer** logs in, searches taxpayers by TIN, reviews returns, and sees a collection report. The project is built over five days and uses fictional data only.

```
Taxpayer or Officer -> Frontend (React) -> API -> Backend logic -> Database -> response back up
```

## 1. What software development is

**Software** is a set of instructions that tells a computer what to do.
*Example:* "When a taxpayer submits a payment, confirm the amount is above zero, record it, and update their balance."

**A system** is many pieces of software working together toward one purpose.
*Example:* The e-Tax platform is a website, a database, a payment gateway connection, and a reporting module acting as one system.

**A developer** is a person who designs, writes, tests, and maintains software.
*Example:* The developer who updates the penalty calculation when a new finance law changes the late-payment rate.

**Software development** is the full process of turning an organization's need into a working, maintained system. Writing code is only one part of it.
*Example:* Taxpayers asking "how do I see what I owe?" becomes a requirement, a design, an API, a database query, a screen, a test, a release, and years of maintenance.

## 2. The Software Development Lifecycle (SDLC)

**The SDLC** is the standard set of stages that almost every real software project passes through. It is drawn as a loop because maintenance feeds the next round of planning.
*Example:* Adding online filing for PAYE returns goes through every stage below, whether or not the team names them.

```
Planning -> Requirements -> Design -> Development -> Testing -> Deployment -> Maintenance -> (back to Planning)
```

**Planning** decides what will be built, why, by when, at what cost, and who owns it.
*Example:* "Taxpayers cannot easily see what they owe. We will build a portal where they can check their balance and pay online."
*Capstone:* The project charter: two roles, three core features, five days, a working demo at the end.

**Stakeholders** are the people and groups affected by the system or involved in building it.
*Example:* Taxpayers, LIRS officers, the IT team, the finance department, and the bank or payment gateway partner.

**Requirements** are the exact statements of what the system must do and how well it must do it.
*Example:* "A taxpayer can pay by card and receives a receipt number."
*Capstone:* "An officer can search for a taxpayer by TIN and see that taxpayer's returns."

**Design** is the plan for how the system will be structured, made before coding begins.
*Example:* Deciding that one taxpayer has many returns, and each return has many payments, before creating any database tables.
*Capstone:* Four tables (Taxpayers, TaxReturns, Payments, ComplianceLogs) and about six API endpoints.

**Development** is writing the code according to the design.
*Example:* Writing the function that calculates a late-payment penalty.
*Capstone:* The payment endpoint (Day 2) and the payment form in React (Day 3).

**Testing** is checking that the system works correctly and handles bad input safely.
*Example:* Submitting a TIN with letters, an empty amount, or a payment of minus ₦5,000, and confirming each is rejected with a clear message.
*Capstone:* Unit tests for the penalty function and the payment endpoint (Day 4).

**Deployment** is putting the system where real users can reach it.
*Example:* Releasing a new filing page to the live portal outside the month-end filing rush.
*Capstone:* The portal running at a published address for the final demo (Day 5).

**Maintenance** is fixing bugs, adding features, and keeping the system healthy over the years.
*Example:* A new tax rate, a new payment channel, or a security patch for a library.
*Capstone:* After the demo, the next backlog item could be SMS receipts for payments.

**The cost of late mistakes.** The later a mistake is found, the more it costs to fix, so the early stages are the cheapest place to get things right.
*Example:* A requirement that wrongly says "amounts in dollars" costs a conversation to fix on paper. Found after go-live, it means corrected records, apologies to taxpayers, and an emergency release.

## 3. Development methodologies

**A methodology** is an agreed way of organizing the work of building software.
*Example:* A team that runs two-week cycles with a demo at the end of each follows an Agile methodology.

**Waterfall** completes each lifecycle stage fully before starting the next, with no returning to earlier stages. It suits work where requirements are fixed and well understood, and struggles when users cannot describe what they need until they see it.
*Example:* Replacing a database server with an identical model under a fixed-price contract is a Waterfall-style job. Requirements are signed off at the start and delivery is at the end.

**Agile** builds software in small working pieces, gathers feedback early, and adjusts as it goes.
*Example:* A taxpayer portal released first with only the balance page. Feedback shows people also want payment history, so that arrives in the next cycle.

**A hybrid approach** keeps fixed contract milestones on the outside and runs short Agile cycles on the inside. Many government projects work this way.
*Example:* LIRS commits to "portal live by the fourth quarter" while the team builds in two-week cycles, with a demo to stakeholders after each.

| | Waterfall | Agile |
|---|---|---|
| Shape | One-way staircase | Repeating loops |
| Change | Hard and costly | Expected |
| Delivery | Everything at the end | Small pieces often |
| Feedback | Late | Early and often |
| Typical LIRS fit | Fixed-contract infrastructure replacement | Taxpayer portal improvements |

**Scrum** is the most widely used way of running Agile. It is built from a few simple parts.

**Product backlog.** The master list of everything wanted, ordered by importance.
*Example:* "Pay by card", "Download receipt", "Officer collection report", "SMS reminders".

**Sprint.** A fixed, short work period, usually one or two weeks.
*Example:* This five-day training week is a single sprint for the capstone.

**Sprint planning.** Choosing which backlog items fit into the coming sprint.
*Example:* Choosing balance, returns list, payment recording, and officer search for this week.

**Sprint backlog.** The items chosen for the current sprint.
*Example:* Stories US-01 to US-07 in the capstone backlog.

**Daily stand-up.** A short daily check where each person says what they did, what they will do, and what is blocking them.
*Example:* "Yesterday I wrote the requirements. Today I build the database. I am blocked until SQL Server is installed."

**Sprint review.** A demonstration of the working result to the people who care about it.
*Example:* The Friday capstone demo.

**Retrospective.** A frank team discussion of how the work went and what to improve.
*Example:* "Two of us edited the README at once and lost time to a conflict. Next time, one person owns each file."

**Scrum roles.** The Product Owner decides priorities, the Scrum Master protects the process and removes blockers, and the Development Team builds.
*Example:* On a real LIRS portal project, a head of department might be Product Owner, a project lead the Scrum Master, and the developers the team.

## 4. Requirements and specifications

**A business need** is the problem described in the organization's own words.
*Example:* "Taxpayers cannot easily see what they owe, so they keep calling the helpdesk."

**A functional requirement** states what the system does.
*Example:* "The system shall show a taxpayer's outstanding balance."

**A non-functional requirement** states how well the system does it: speed, security, availability, or usability.
*Example:* "The balance page loads in under 3 seconds on a normal mobile connection."

| Statement | Type |
|---|---|
| An officer can search by TIN. | Functional |
| The portal works on a mobile phone. | Non-functional |
| The system sends an SMS after a payment. | Functional |
| The system handles 5,000 users at month-end. | Non-functional |
| Passwords are never stored as plain text. | Non-functional (security) |

**A technical specification** is the developer's translation of requirements into concrete tables, endpoints, and rules.
*Example:* The balance requirement becomes `GET /api/taxpayers/{id}/balance`, which reads TaxReturns and Payments and returns the difference.

**A user story** is a requirement told from the user's point of view, in one sentence.

```
As a <who>, I want <what>, so that <why>.
```
*Example:* "As a taxpayer, I want to see my outstanding balance, so that I know how much to pay."

**Acceptance criteria** are the checklist that proves a story is finished. Each criterion should be testable.
*Example:* For the balance story:
- On my dashboard I see my outstanding balance in naira, for example ₦245,000.00.
- If I owe nothing, I see "₦0.00, you are up to date".
- The balance loads in under 3 seconds on a normal mobile connection.
- I cannot see any other taxpayer's balance.

**From story to specification.** The same requirement is written twice: once for the user, once for the developer.

| Piece | Specification |
|---|---|
| Endpoint | `GET /api/taxpayers/{id}/balance` |
| Reads | TaxReturns (tax due) and Payments (amount paid) |
| Rule | balance = total tax due minus total paid |
| Response | `{ "taxpayerId": 101, "balance": 245000.00 }` |
| Security | the user must be logged in and must own the record |

**A task is not a story.** A story describes value to a user. A task describes work for a developer.
*Example:* "Create the Payments table" is a task. "As a taxpayer, I want to record a payment so that my balance goes down" is a story.

**Testable criteria.** A criterion that cannot be checked is not a criterion.
*Example:* "The system is fast" cannot be tested. "The balance loads in under 3 seconds" can.

## 5. Clean code and documentation

**Clean code** is code written so that other people can read it, understand it, and change it safely.
*Example:* `calculate_late_penalty(amount_due_naira, days_late)` instead of `p(a, d)`.

**Maintainability** is how easily someone other than the original author can change the code without breaking it.
*Example:* Changing the VAT rate from 7.5% to 8% is a one-line change in one place, not a search through forty files.

**Refactoring** is improving the structure of code without changing what it does.
*Example:* Renaming and splitting the penalty function while it still returns the same amounts.

**A coding standard** is the team's agreed style, so all the code looks and behaves alike.
*Example:* "Python uses snake_case, C# uses PascalCase for methods, and money amounts always use decimal types."

**Meaningful names.** A name should say what the thing is or does.
*Example:* `outstanding_balance_naira` instead of `x`.

**One function, one job.** Each function does a single, clear task.
*Example:* Validating a TIN, saving a payment, sending an SMS, and building a receipt are four functions, not one.

```python
def validate_tin(tin): ...
def save_payment(payment): ...
def send_payment_sms(phone, amount): ...
def build_receipt(payment): ...
```

**No magic numbers.** Numbers with a meaning get a named constant.
*Example:* `LATE_FILING_FEE_NAIRA = 500` instead of a bare `500` buried in a formula.

**Comments explain why, not what.**
*Example:* `# Late fee set by the current finance act, confirm with the policy unit before changing` helps a future reader. `# add 100 per day` repeats the code.

**Before and after.** The late-payment rule (10% of the amount due plus ₦100 per day late, waived by a flag), first unreadable:

```python
def p(a, d, x):
    r = 0
    if d > 0:
        r = a * 0.1 + d * 100
    if x == 1:
        r = 0
    return r
```

Then clean, with the same behaviour:

```python
PENALTY_RATE = 0.10
DAILY_LATE_FEE_NAIRA = 100

def calculate_late_penalty(amount_due_naira, days_late, is_waived):
    if is_waived or days_late <= 0:
        return 0

    percentage_penalty = amount_due_naira * PENALTY_RATE
    daily_penalty = days_late * DAILY_LATE_FEE_NAIRA
    return percentage_penalty + daily_penalty
```
A new law that sets the rate at 12% now changes one constant, and every caller follows.

**Documentation** is the written material that lets someone understand and run a system without its author.
*Example:* When a developer leaves LIRS, the README and API notes are what keep the system maintainable.

**README.** Says what the project is, how to run it, and how to test it.
*Example:* "LIRS Taxpayer Mini-Portal. Run the backend with `dotnet run`. Run the frontend with `npm run dev`."

**Inline comments.** Short notes where the reason for the code is not obvious.
*Example:* A comment on why a payment is rejected when it exceeds the amount owed.

**API documentation.** A list of endpoints with their inputs and outputs.
*Example:* `GET /api/taxpayers/{id}/balance` returns the balance as JSON.

**Changelog.** A record of what changed in each release.
*Example:* "v0.2: added payment receipts; fixed rounding of late penalties."

## 6. Version control with Git

**Version control** is a system that records every change to a set of files, who made it, when, and why.
*Example:* Instead of folders named `penalty_final_v3_NEW`, one file with a full, trustworthy history.

**Git** is the most widely used version control tool. It runs on the developer's own computer.
*Example:* Every edit to the capstone's `requirements.md` is recorded and can be reviewed or reversed.

**GitHub** is a website that hosts Git repositories so teams can share them. GitLab and Azure DevOps do the same job.
*Example:* LIRS could host its code on Azure DevOps, and every Git command stays identical.

**A repository (repo)** is the project folder plus its full history.
*Example:* `lirs-taxpayer-portal`.

**A commit** is a saved snapshot of the changes, with a message describing them.
*Example:* "Add acceptance criteria for payment story US-04". A message like "fixed stuff" says nothing, so good messages state what changed and why.

**A branch** is a separate line of work, like a photocopy that can be edited without touching the original.
*Example:* `feature/US-02-balance` holds work on the balance story while `main` stays working.

**A merge** brings the work of one branch into another.
*Example:* Merging `feature/US-02-balance` into `main` once it has been reviewed.

**Clone, push, and pull.** Clone copies a remote repository to your computer, push sends your commits to the remote, and pull brings other people's commits to you.
*Example:* One developer pushes the requirements, and a colleague pulls them to their own laptop.

```
My laptop (local repo)  <-- push / pull -->  GitHub (remote repo)  <-- push / pull -->  Colleague's laptop
```

**A pull request (PR)** asks the team to review a branch before it is merged. It also leaves a record of who proposed and approved each change.
*Example:* "Please review my requirements for US-03 before they go into `main`." For LIRS, that record doubles as audit evidence for rule changes.

**A merge conflict** occurs when two people change the same lines of a file and Git cannot choose between them. It is resolved by a person, not an error.
*Example:* Two developers each reword the sprint goal in the README. Git marks both versions:

```
<<<<<<< HEAD
Sprint goal: a taxpayer can see their balance and pay online.
=======
Sprint goal: a taxpayer can view balance, returns, and payments.
>>>>>>> feature/US-07-report
```
The markers are removed, the combined wording is kept, and the file is committed:
"Sprint goal: a taxpayer can view their balance and returns, and pay online."

**The core commands.** Each does one small job.

| Command | What it does | Example |
|---|---|---|
| `git clone <url>` | Copies a remote repo to your computer | Copy `lirs-taxpayer-portal` from GitHub |
| `git status` | Shows what has changed | See that `requirements.md` is modified |
| `git add <file>` | Stages a change for the next commit | Stage `docs/requirements.md` |
| `git commit -m "message"` | Saves a snapshot with a message | "Add stories US-01 to US-05" |
| `git push origin <branch>` | Sends commits to the remote | Send the commit to GitHub |
| `git pull origin main` | Brings the latest remote work to you | Receive a colleague's backlog update |
| `git checkout -b <branch>` | Creates and switches to a new branch | `feature/US-06-officer-review` |
| `git log --oneline` | Lists the history, one line per commit | Review the day's commits |

**The `.gitignore` file** lists files Git must never track: dependencies, build output, and secrets.
*Example:* `.env` holds the database password and must never reach GitHub.

```
node_modules/
bin/
obj/
.env
*.log
```

**The team workflow** keeps shared work safe.
1. `main` is always working and protected. *Example:* the demo version of the portal always runs.
2. Each task gets its own branch. *Example:* `feature/US-03-returns`.
3. Commits are small and frequent. *Example:* one commit per story added to `requirements.md`.
4. Every change goes through a pull request with a reviewer. *Example:* a colleague checks the US-05 story before it merges.
5. After merging, the branch is deleted and everyone pulls the latest `main`. *Example:* both pull before starting US-04.

## Capstone work for Day 1

By the end of Day 1 the capstone has no running application yet, but it has a documented plan, agreed standards, working starter code, and a Git history showing a real team workflow.

### The repository

```
lirs-taxpayer-portal/
  README.md
  .gitignore
  docs/
    project-charter.md
    backlog.md
    requirements.md
    coding-standards.md
  backend/
    starter/
      taxpayers.py
  frontend/        (built on Day 3)
```

### Project charter: `docs/project-charter.md`

```markdown
# Project Charter: LIRS Taxpayer Mini-Portal

## Problem
Taxpayers cannot easily see what they owe or pay online, and officers lack a quick view of collections.

## Goal
Build a working mini-portal in five days that demonstrates the full software lifecycle.

## Stakeholders
- Taxpayer: sees balance, views returns, makes payments
- LIRS Officer: searches taxpayers, reviews returns, sees collection report
- Admin (stretch): manages users and roles

## Scope
In: login with two roles, taxpayer balance, returns and payments, officer search and report
Out: real payment gateway, real taxpayer data, mobile app

## Success looks like
- The portal runs end to end on the final day using fictional data
- Tests pass, and no secrets are stored in the repository

## Timeline
Day 1 foundation, Day 2 backend, Day 3 frontend, Day 4 quality and security, Day 5 deployment and demo
```

### Product backlog: `docs/backlog.md`

| ID | As a... | I want to... | Priority | Planned |
|---|---|---|---|---|
| US-01 | Taxpayer | Log in securely | High | Day 3 |
| US-02 | Taxpayer | See my outstanding balance | High | Days 2 and 3 |
| US-03 | Taxpayer | View my tax returns | High | Days 2 and 3 |
| US-04 | Taxpayer | Record a payment | High | Days 2, 3, and 4 |
| US-05 | Officer | Search a taxpayer by TIN | High | Days 2 and 3 |
| US-06 | Officer | Review and approve a submitted return | Medium | Day 5 |
| US-07 | Officer | See total collected per state | Medium | Day 5 |
| US-08 | Admin | Manage user roles | Low (stretch) | If time allows |

Sprint goal: by the final day, a taxpayer can log in, see their balance, and make a payment, and an officer can find a taxpayer and see a collection report.

### Requirements: `docs/requirements.md`

```markdown
# Requirements: LIRS Taxpayer Mini-Portal

## US-01 Taxpayer login
As a taxpayer, I want to log in with my TIN and password, so that I can see my records privately.
Acceptance criteria
- A valid TIN and password opens my dashboard
- Wrong details show "Invalid TIN or password" without saying which part was wrong
- After 5 wrong attempts the account is locked for 15 minutes
- Passwords are stored hashed, never as plain text (non-functional, security)
Specification: POST /api/auth/login returns a token containing the user's id and role

## US-02 Taxpayer balance
As a taxpayer, I want to see my outstanding balance, so that I know how much to pay.
Acceptance criteria
- My dashboard shows my balance in naira, for example ₦245,000.00
- If I owe nothing, I see "₦0.00, you are up to date"
- The balance loads in under 3 seconds on a normal mobile connection (non-functional)
- I cannot see any other taxpayer's balance (non-functional, security)
Specification: GET /api/taxpayers/{id}/balance

## US-03 Taxpayer returns
As a taxpayer, I want to see my tax returns for each year, so that I can confirm what I filed.
Acceptance criteria
- I see a list of year, declared income, tax due, and status
- Amounts show in naira with separators, for example ₦1,250,000.00
- I only see my own returns (non-functional, security)
Specification: GET /api/taxpayers/{id}/returns

## US-04 Taxpayer payment
As a taxpayer, I want to record a payment against a return, so that my balance goes down.
Acceptance criteria
- I choose a return, enter an amount, and select a channel (Bank, Card, USSD)
- The amount must be above zero and not more than the balance owed
- I receive a receipt number
- A failed payment shows a clear message (non-functional, usability)
Specification: POST /api/payments validates, saves, and returns a payment id

## US-05 Officer search by TIN
As a LIRS officer, I want to search for a taxpayer by TIN, so that I can review their records quickly.
Acceptance criteria
- A TIN search returns the taxpayer, or "No taxpayer found"
- Results appear in under 2 seconds (non-functional)
- Only officers can use this search (non-functional, security)
Specification: GET /api/taxpayers?tin=...

## US-06 Officer return review
As a LIRS officer, I want to review and approve a submitted return, so that the taxpayer's record is accurate.
Acceptance criteria
- I see submitted returns waiting for review
- Approving a return changes its status and is recorded in the compliance log with my name and the time
- A taxpayer cannot approve their own return (non-functional, security)
Specification: PATCH /api/returns/{id}/status

## US-07 Officer collection report
As a LIRS officer, I want to see total collections per state, so that I can report revenue performance.
Acceptance criteria
- The report lists each state and the total paid in naira
- The report loads in under 5 seconds (non-functional)
- Only officers can open the report (non-functional, security)
Specification: GET /api/reports/collections-by-state
```

### Coding standards: `docs/coding-standards.md`

```markdown
# Coding Standards: LIRS Taxpayer Mini-Portal

## Naming
- Names say what a thing is: outstanding_balance_naira, not x
- Python: snake_case. C#: PascalCase for classes and methods, camelCase for variables. JavaScript and React: camelCase, with components in PascalCase

## Functions
- One function, one job
- Short enough to read without scrolling

## Constants
- No magic numbers. Rates, fees, and limits are named constants

## Money
- Use decimal types for naira amounts, never floating point in C#
- Display with separators and the naira sign: ₦1,250,000.00

## Comments
- Comment the reason, not the action

## Security basics
- Never commit passwords, keys, or connection strings
- Never build SQL by joining strings

## Git
- main is always working
- One branch per story: feature/US-02-balance
- Commit messages say what changed and why
- Every change goes through a pull request with one reviewer
```

### README: `README.md`

```markdown
# LIRS Taxpayer Mini-Portal

A training project: a small taxpayer portal built over five days to practise the full software lifecycle. Uses fictional data only.

## What it does
- Taxpayers: log in, see balance, view returns, record payments
- Officers: search by TIN, review returns, see collection report

## Structure
- docs/ charter, backlog, requirements, coding standards
- backend/ API and database (Day 2)
- frontend/ React portal (Day 3)

## How to run
Coming on Days 2 and 3.
```

### Starter code: `backend/starter/taxpayers.py`

The first real code in the capstone: fictional taxpayer data, a lookup by TIN, and two business rules written to the clean-code standard.

```python
# Fictional data only. Real taxpayer data is never used in this project.
TAXPAYERS = [
    {"tin": "1000000001", "name": "Adewale Ventures Ltd", "type": "Business", "state": "Lagos"},
    {"tin": "1000000002", "name": "Chioma Okafor", "type": "Individual", "state": "Lagos"},
    {"tin": "1000000003", "name": "Bello Logistics", "type": "Business", "state": "Lagos"},
]

PENALTY_RATE = 0.10
DAILY_LATE_FEE_NAIRA = 100


def find_taxpayer_by_tin(tin):
    """Return the taxpayer with this TIN, or None if there is no match."""
    for taxpayer in TAXPAYERS:
        if taxpayer["tin"] == tin:
            return taxpayer
    return None


def calculate_balance(tax_due_naira, payments_naira):
    """Outstanding balance: total tax due minus everything paid so far."""
    return tax_due_naira - sum(payments_naira)


def calculate_late_penalty(amount_due_naira, days_late, is_waived):
    # Late fee set by the current finance act, confirm with the policy unit before changing
    if is_waived or days_late <= 0:
        return 0

    percentage_penalty = amount_due_naira * PENALTY_RATE
    daily_penalty = days_late * DAILY_LATE_FEE_NAIRA
    return percentage_penalty + daily_penalty


if __name__ == "__main__":
    taxpayer = find_taxpayer_by_tin("1000000001")
    print(taxpayer["name"])
    print(calculate_balance(500000, [150000, 100000]))   # 250000
    print(calculate_late_penalty(100000, 5, False))      # 10500
```

### The Git history at the end of the day

| Branch or step | What it contains |
|---|---|
| `main` (first commits) | Charter, backlog, requirements, coding standards, README, `.gitignore` |
| `feature/US-06-officer-review` | The US-06 story added to `requirements.md` |
| `feature/US-07-report` | The US-07 story added, plus a README edit that conflicts with another branch |
| Pull requests | Each branch reviewed before merging into `main` |
| Merge conflict | Resolved by combining the two sprint-goal wordings in the README |
| `feature/starter-code` | `backend/starter/taxpayers.py`, merged through a pull request |

### State of the capstone at the end of Day 1

| Deliverable | Status |
|---|---|
| Project charter | Complete |
| Backlog and sprint goal | Complete |
| Requirements for US-01 to US-07 | Complete |
| Coding standards and README | Complete |
| Starter code: lookup, balance, late penalty | Complete |
| Repository with protected `main`, pull requests, and a resolved conflict | Complete |
| Database and API | Day 2 |
