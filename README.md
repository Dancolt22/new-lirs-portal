# LIRS Taxpayer Mini-Portal

A training project: a small taxpayer portal built over five days to practise the full software lifecycle for the Lagos State Internal Revenue Service (LIRS) IT team. Uses fictional data only.

## What it does
- Taxpayers: log in, see outstanding liability balance, view filed returns, record payments
- Officers: search taxpayers by TIN, review returns, view revenue collection reports
- Administrators: manage system roles and security (stretch)

## Architecture
- **Frontend:** React (JavaScript, Vite, React Router) running on `http://localhost:5173`
- **Backend API:** ASP.NET Core Web API (.NET 8/10, C# 12) running on HTTP
- **Database:** Microsoft SQL Server (`.\SQLEXPRESS`), accessed via Dapper with parameterized queries
- **Testing:** xUnit unit tests and Postman regression suite

## Project Structure
- `docs/` — Project charter, product backlog, functional/technical requirements, and coding standards
- `backend/` — SQL schema, seed data, ASP.NET Core Web API, and Postman collections
- `frontend/` — React Vite portal application (added Day 3)

## Fictional Seed Data Summary
- **101 (`adewale`):** Adewale Ventures Ltd (Business, Lagos) — Balance: ₦700,000.00
- **102 (`chioma`):** Chioma Okafor (Individual, Lagos) — Balance: ₦0.00 (Fully Settled)
- **103 (`bello`):** Bello Logistics (Business, Lagos) — Balance: ₦600,000.00
- **104 (`ngozi`):** Ngozi Textiles (Business, Ogun) — Balance: ₦300,000.00
- **105 (`emeka`):** Emeka Eze Consulting (Individual, Lagos) — Balance: ₦300,000.00
- **106 (`fatima`):** Fatima Aliyu Foods (Individual, Lagos) — Balance: ₦0.00 (Fully Settled)
- **107 (`tunde`):** Tunde Bakare Enterprises (Business, Lagos) — Balance: ₦500,000.00
- **108 (`amaka`):** Amaka Johnson Creative (Individual, Lagos) — Balance: ₦200,000.00
- **109 (`ibrahim`):** Ibrahim Musa Haulage (Business, Lagos) — Balance: ₦750,000.00
- **110 (`kemi`):** Kemi Adeyemi & Partners (Business, Lagos) — Balance: ₦300,000.00
- **111 (`olumide`):** Olumide Solar Systems Ltd (Business, Lagos) — Balance: ₦900,000.00
- **112 (`zainab`):** Zainab Farouk Pharmacy (Individual, Lagos) — Balance: ₦0.00 (Fully Settled)
- **Officers:** `bisi`, `folake` (LIRS Revenue Officers)
- **Default Password for all accounts:** `pass123`
