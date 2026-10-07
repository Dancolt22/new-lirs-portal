# Requirements: LIRS Taxpayer Mini-Portal

## US-01 Taxpayer login
As a taxpayer, I want to log in with my username and password, so that I can see my records privately.
### Acceptance criteria
- A valid username and password opens my dashboard
- Wrong details show "Invalid username or password" without saying which part was wrong
- After 5 wrong attempts the account is locked for 15 minutes
- Passwords are stored hashed, never as plain text (non-functional, security)
**Specification:** POST `/api/auth/login` returns a token containing the user's id and role.

## US-02 Taxpayer balance
As a taxpayer, I want to see my outstanding balance, so that I know how much to pay.
### Acceptance criteria
- My dashboard shows my balance in naira, for example ₦700,000.00
- If I owe nothing, I see "₦0.00, you are up to date"
- The balance loads in under 3 seconds on a normal mobile connection (non-functional)
- I cannot see any other taxpayer's balance (non-functional, security)
**Specification:** GET `/api/taxpayers/{id}/balance` returns `{ "taxpayerId": 101, "balance": 700000.00 }`.

## US-03 Taxpayer returns
As a taxpayer, I want to see my tax returns for each year, so that I can confirm what I filed.
### Acceptance criteria
- I see a list of year, declared income, tax due, and status
- Amounts show in naira with separators, for example ₦500,000.00
- I only see my own returns (non-functional, security)
**Specification:** GET `/api/taxpayers/{id}/returns` returns that taxpayer's returns ordered by newest year first.

## US-04 Taxpayer payment
As a taxpayer, I want to record a payment against a return, so that my balance goes down.
### Acceptance criteria
- I choose a return, enter an amount, and select a channel (Bank, Card, USSD)
- The amount must be above zero and not more than the balance owed
- I receive an official receipt number starting with `RCT-`
- A failed payment shows a clear message (non-functional, usability)
**Specification:** POST `/api/payments` validates, saves, and returns `{ "paymentId": 5, "receiptNumber": "RCT-000005", "status": "Successful" }`.

## US-05 Officer search by TIN
As a LIRS officer, I want to search for a taxpayer by TIN, so that I can review their records quickly.
### Acceptance criteria
- A TIN search returns the taxpayer, or "No taxpayer found"
- Results appear in under 2 seconds (non-functional)
- Only officers can use this search (non-functional, security)
**Specification:** GET `/api/taxpayers?tin=...`

## US-06 Officer return review
As a LIRS officer, I want to review and approve a submitted return, so that the taxpayer's record is accurate.
### Acceptance criteria
- I see submitted returns waiting for review
- Approving a return changes its status and is recorded in the compliance log with officer name and timestamp
- A taxpayer cannot approve their own return (non-functional, security)
**Specification:** PATCH `/api/returns/{id}/status`

## US-07 Officer collection report
As a LIRS officer, I want to see total collections per state, so that I can report revenue performance.
### Acceptance criteria
- The report lists each state and the total paid in naira
- The report loads in under 5 seconds (non-functional)
- Only officers can open the report (non-functional, security)
**Specification:** GET `/api/reports/collections-by-state`
