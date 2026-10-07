# Coding Standards: LIRS Taxpayer Mini-Portal

## Naming
- Names say what a thing is: `outstanding_balance_naira`, not `x`
- Python: `snake_case`
- C#: `PascalCase` for classes and methods, `camelCase` for local variables and parameters
- JavaScript / React: `camelCase` for functions and variables, `PascalCase` for components

## Functions
- One function, one job
- Short enough to read without scrolling

## Constants
- No magic numbers. Rates, fees, and limits are named constants (e.g. `PENALTY_RATE = 0.10`)

## Money
- Use `decimal` in C# for currency, never `float` or `double`
- Display with thousands separators and the naira sign: `₦1,250,000.00`

## Comments
- Comment the reason (why), not the action (what)

## Security Basics
- Never commit passwords, API keys, or connection strings to Git
- Never build SQL by joining or concatenating strings; always use parameterized queries
- Uniform safe error responses to API clients: `{ "error": "...", "code": "..." }`

## Git Workflow
- `main` branch is always working and deployable
- One feature branch per story: `feature/<story-name>`
- Commit messages state what changed and why
- Every change is merged through a pull request with code review
