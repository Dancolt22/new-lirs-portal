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
- **101:** Adewale Ventures Ltd (Business, Lagos) — Balance: ₦700,000.00
- **102:** Chioma Okafor (Individual, Lagos) — Balance: ₦0.00
- **103:** Bello Logistics (Business, Lagos) — Balance: ₦600,000.00
- **104:** Ngozi Textiles (Business, Ogun) — Balance: ₦300,000.00
