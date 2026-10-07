# Day 3 FINAL Instructor Guide: Frontend Development and User Experience for the LIRS Taxpayer Mini-Portal

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

Today's goal: build a **modern, responsive web interface** using **React** and **Vite** that connects to yesterday's ASP.NET Core API. By 16:00, our system will feature a branded login screen, a taxpayer dashboard showing real balances and returns, an interactive payment form that updates balances live, and an officer dashboard for TIN lookups.

```
Taxpayer / Officer (Browser at http://localhost:5173)
       │
       ▼ [HTTP Fetch Requests: JSON]
┌────────────────────────────────────────────────────────────────────────┐
│ ASP.NET Core API (http://localhost:5123)                               │
│                                                                        │
│  1. CORS Policy ("portal") ──► Allows requests from localhost:5173     │
│  2. Controllers            ──► TaxpayersController, PaymentsController │
│  3. Business Logic         ──► PaymentService (Rule Validation)        │
│  4. Data Access            ──► Dapper & Microsoft.Data.SqlClient       │
└────────────────────────────────────┬───────────────────────────────────┘
                                     │
                                     ▼
                        SQL Server (.\SQLEXPRESS)
                        Database: LirsPortal
```

**EXPLAIN (Say this at 09:00):**
- "Yesterday we built the backend: the database and the API doorway. But taxpayers and revenue officers do not interact with raw JSON or Postman. They interact with web screens."
- "Today we cover the complete frontend stack: **HTML** for the skeleton, **CSS** for styling and layout, **JavaScript** for browser logic, and **React** for organizing screens into reusable, reactive components."
- "By the end of the day, when you record a tax payment on the screen, that payment travels through our API into SQL Server, and the screen automatically refreshes to show your new reduced balance."
- "**Honest Caveat:** Today's login is a front-end simulation using fixed training users so we can build the user experience. Real server-side authentication, password hashing, and cryptographic JWT tokens are added tomorrow on Day 4. Hiding a button in React is user experience, not security!"

### Words used today, in one line each

| Word | Plain meaning |
|---|---|
| **HTML** | The markup language that defines the elements and structure of a page (headings, forms, tables). |
| **CSS** | The stylesheet language that defines how HTML looks (colors, fonts, padding, Flexbox, Grid). |
| **JavaScript** | The programming language that runs inside the browser to handle clicks, calculations, and network calls. |
| **DOM (Document Object Model)** | The live, in-memory tree of page elements that JavaScript can read and change. |
| **Event** | An action taken by the user (a mouse click, typing in an input, submitting a form). |
| **JSON** | A text format for exchanging data between browser and server (`{"tin": "1000000001"}`). |
| **fetch** | The built-in browser JavaScript function used to send HTTP requests to an API. |
| **async / await** | JavaScript syntax that allows code to wait for slow network requests without freezing the screen. |
| **Node.js** | A program that lets developers run JavaScript on the computer outside the browser (used for tooling). |
| **npm** | The Node Package Manager used to install frontend libraries (such as React and React Router). |
| **React** | A popular JavaScript library for building user interfaces out of reusable components. |
| **Component** | A reusable JavaScript function that returns HTML-like markup (JSX) to display on screen. |
| **JSX** | The syntax allowing HTML tags directly inside JavaScript code (`return <h1>Hello</h1>;`). |
| **Props** | Inputs passed down from a parent component to a child component (read-only parameters). |
| **State (`useState`)** | Data a component remembers; whenever state changes, React redraws the component automatically. |
| **`useEffect`** | A React hook that runs code after a component appears on screen (where we fetch data from our API). |
| **Context** | A way to share global data (like the logged-in user) across all components without manual passing. |
| **Routing (`react-router-dom`)** | Showing different pages at different URL paths (`/login`, `/taxpayer`, `/officer`) without a page reload. |
| **CORS** | Cross-Origin Resource Sharing: a browser security rule requiring APIs to explicitly grant permission to web apps. |

---

## 1. Software and Pre-Flight Checks

Before opening project files, verify that Node.js, npm, and required extensions are ready.

**GO TO: PowerShell terminal inside VS Code**

Type each command and verify the output:

```powershell
node --version
npm --version
dotnet --version
git --version
```

### YOU SHOULD SEE:
- `node`: `v18.xx.x` or `v20.xx.x` (LTS).
- `npm`: `9.xx.x` or `10.xx.x`.
- `dotnet`: `8.0.xxx` or newer.
- `git`: `2.xx.x`.

### IF IT FAILS:
| Symptom | Cause | Fix |
|---|---|---|
| `node: The term 'node' is not recognized` | Node.js not installed or PATH not updated | Download and run the Node.js LTS installer from `nodejs.org`. Restart VS Code after install. |
| `npm: The term 'npm' is not recognized` | npm not in PATH | Restart VS Code and PowerShell terminal. |
| Script execution policy error | PowerShell blocks npm execution | Run `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` in the terminal. |

**VS Code Extensions Check:**
Click the Extensions icon on the left (Ctrl+Shift+X) and verify these are installed:
1. `ES7+ React/Redux/React-Native snippets`
2. `Prettier - Code formatter`
3. `Auto Rename Tag`

---

## 2. Morning Sanity Check: Starting from the Day 2 End State

Confirm that our backend API and SQL Server database from Day 2 are running and ready.

### Step 1: Check Git working branch
**GO TO: VS Code terminal**
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal
git status
```
Ensure you are on `main` (or yesterday's merged feature branch). If there are uncommitted changes, run `git stash`.

### Step 2: Start the backend API and note the port
**GO TO: VS Code terminal**
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\backend\LirsPortal.Api
dotnet run --launch-profile http
```

### YOU SHOULD SEE:
```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5123
```
*(CRITICAL: Note down the exact port printed! For example, `5123`. We will configure this port in our React frontend in Step 4).*

### Step 3: Quick Postman verification
**GO TO: Postman**
Send a `GET` request to `http://localhost:5123/api/taxpayers`.
- Verify it returns HTTP status `200 OK` with our 4 taxpayers (Adewale, Chioma, Bello, Ngozi).
- Keep this terminal window running the API. Open a **second terminal tab** in VS Code for all frontend commands.

---

## 3. PART ONE: Enable CORS on the Backend API (VS Code)

**EXPLAIN:**
- "By default, web browsers enforce the **Same-Origin Policy**. If our React app runs on `http://localhost:5173` and tries to call our API on `http://localhost:5123`, the browser blocks the call with a CORS error because the port numbers differ."
- "To fix this, we instruct ASP.NET Core to explicitly trust calls originating from `http://localhost:5173`."

### Step 1: Update `Program.cs` in `LirsPortal.Api`
**GO TO: VS Code**
1. Stop the running API in the terminal using **Ctrl+C**.
2. Open `backend/LirsPortal.Api/Program.cs`.
3. Locate the line:
   ```csharp
   builder.Services.AddControllers();
   ```
4. Immediately below it, add the CORS policy configuration:
   ```csharp
   builder.Services.AddCors(options =>
       options.AddPolicy("portal", policy =>
           policy.WithOrigins("http://localhost:5173")
                 .AllowAnyHeader()
                 .AllowAnyMethod()));
   ```
5. Locate the line:
   ```csharp
   app.MapControllers();
   ```
6. Immediately above it, add the CORS middleware:
   ```csharp
   app.UseCors("portal");
   ```

### Full updated `Program.cs` for Day 3:
```csharp
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using LirsPortal.Api.Repositories;
using LirsPortal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Allow React frontend origin
builder.Services.AddCors(options =>
    options.AddPolicy("portal", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<TaxpayerRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<PaymentService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = 500;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new ApiError("Something went wrong. Please try again.", "SERVER_ERROR"));
}));

app.UseCors("portal");

app.MapControllers();

app.Run();
```

### Step 2: Restart the API
**GO TO: VS Code terminal**
```powershell
dotnet run --launch-profile http
```
Leave it running.

---

## 4. PART TWO: Quick Practice — The Standalone Tax Estimator (`estimator.html`)

Before jumping into React, spend 15 minutes showing how HTML, CSS, and JavaScript work together in a single file.

**EXPLAIN:**
- "Every web framework compiles down to HTML, CSS, and JavaScript. Let us build a 1-file tax estimator to see how JavaScript reads a user's input, runs a 10% tax calculation, and dynamically updates the screen."

### Step 1: Create `estimator.html`
**GO TO: VS Code**
1. Right-click the `frontend` folder (or Desktop), select **New File**, and name it `estimator.html`.
2. Paste the following complete HTML file:

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>LIRS Quick Tax Estimator</title>
  <style>
    body { font-family: Arial, sans-serif; background: #f5f7fa; margin: 0; padding: 20px; }
    .card { max-width: 420px; margin: 40px auto; background: white; padding: 24px;
            border-radius: 8px; border: 1px solid #ddd; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
    h1 { color: #1a3c6e; margin-top: 0; font-size: 22px; }
    label { font-weight: bold; font-size: 14px; }
    input, button { width: 100%; padding: 12px; margin-top: 8px; font-size: 16px; box-sizing: border-box; }
    input { border: 1px solid #ccc; border-radius: 4px; }
    button { background: #1a3c6e; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: bold; }
    button:hover { background: #12294d; }
    #result { margin-top: 20px; font-weight: bold; font-size: 18px; }
    .error { color: #b00020; }
    .success { color: #1b7f3b; }
  </style>
</head>
<body>
  <div class="card">
    <h1>LIRS Quick Tax Estimator</h1>
    <label for="income">Declared Annual Income (₦)</label>
    <input id="income" type="number" placeholder="e.g. 5000000" min="1">
    <button id="calc">Calculate 10% Statutory Tax</button>
    <p id="result"></p>
  </div>

  <script>
    const button = document.querySelector("#calc");
    const result = document.querySelector("#result");

    button.addEventListener("click", () => {
      const income = Number(document.querySelector("#income").value);

      if (income <= 0) {
        result.textContent = "Please enter an income greater than zero.";
        result.className = "error";
        return;
      }

      const tax = income * 0.10;
      result.textContent = `Estimated Tax Due: ₦${tax.toLocaleString('en-NG', { minimumFractionDigits: 2 })}`;
      result.className = "success";
    });
  </script>
</body>
</html>
```

### Step 2: Open and test in the browser
1. Right-click `estimator.html` in VS Code -> **Reveal in File Explorer**.
2. Double-click `estimator.html` to open it in Chrome or Edge.
3. Type `5000000` into the income box and click **Calculate 10% Statutory Tax**.
4. Press **F12** to open DevTools, click **Console**, and show the student where browser logs and errors appear.

### YOU SHOULD SEE:
`Estimated Tax Due: ₦500,000.00` in green text.

---

## 5. PART THREE: Create the Vite React Project

Now we scaffold the production React application using Vite.

**EXPLAIN:**
- "In enterprise development, we do not write entire applications in single HTML files. We use **Vite** to create a modular project. React splits our screens into reusable components like `TopBar`, `Naira`, and `ProtectedRoute`."

### Step 1: Scaffold the project
**GO TO: VS Code terminal (Tab 2, while API runs in Tab 1)**
Navigate to `frontend/`:

```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal\frontend
npm create vite@latest portal -- --template react
```

*When prompted to proceed, press **Enter** (or `y`).*

### Step 2: Install dependencies
Navigate into the newly created `portal` folder and install packages:

```powershell
cd portal
npm install
npm install react-router-dom
```

### Step 3: Clean up Vite boilerplate files
Remove sample CSS and assets that we do not need:

```powershell
Remove-Item src\App.css -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force src\assets -ErrorAction SilentlyContinue
```

### Step 4: Create project folder structure
Create the four clean architectural folders inside `src/`:

```powershell
New-Item -ItemType Directory -Path src\components -Force
New-Item -ItemType Directory -Path src\pages -Force
New-Item -ItemType Directory -Path src\services -Force
New-Item -ItemType Directory -Path src\context -Force
```

### Step 5: Test Vite server startup
```powershell
npm run dev
```

### YOU SHOULD SEE:
```text
  VITE v5.x.x  ready in 250 ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: use --host to expose
```
Open `http://localhost:5173` in Chrome or Edge to confirm the initial Vite page loads. Leave this server running!

---

## 6. PART FOUR: Step-by-Step Code Files

We will now write the 10 code files that form the complete application.

```
frontend/portal/src/
  ├── index.css                   (Global styles & responsive layout)
  ├── services/
  │   └── api.js                  (Centralized fetch API client)
  ├── context/
  │   └── AuthContext.jsx         (Global login state & simulation)
  ├── components/
  │   ├── Naira.jsx               (Currency formatting component)
  │   ├── TopBar.jsx              (Header banner with user info & logout)
  │   └── ProtectedRoute.jsx      (Role-based access boundary)
  ├── pages/
  │   ├── LoginPage.jsx           (Sign in screen)
  │   ├── TaxpayerDashboard.jsx   (Balances, returns table & payment form)
  │   └── OfficerDashboard.jsx    (TIN search & balance inspection)
  ├── App.jsx                     (Router and route mapping)
  └── main.jsx                    (React bootstrap entry point)
```

---

### File 1: Global Styles (`src/index.css`)
**EXPLAIN:**
- "This stylesheet provides clean typography, card layouts, scrollable table wrappers for mobile phones, and status colors (red for errors, green for success, LIRS navy `#1a3c6e` for branding)."

**GO TO: VS Code**
Open `frontend/portal/src/index.css` (replace its entire content):

```css
* { 
  box-sizing: border-box; 
}

body { 
  font-family: Arial, sans-serif; 
  margin: 0; 
  background: #f5f7fa; 
  color: #222; 
}

.container { 
  max-width: 900px; 
  margin: 0 auto; 
  padding: 16px; 
}

.card { 
  background: white; 
  border: 1px solid #ddd; 
  border-radius: 8px; 
  padding: 20px; 
  margin-bottom: 20px; 
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}

.topbar { 
  display: flex; 
  justify-content: space-between; 
  align-items: center; 
  background: #1a3c6e; 
  color: white; 
  padding: 14px 20px; 
}

button { 
  background: #1a3c6e; 
  color: white; 
  border: none; 
  border-radius: 6px; 
  padding: 10px 18px; 
  cursor: pointer; 
  font-size: 14px;
  font-weight: bold;
}

button:hover { 
  background: #12294d; 
}

button:disabled { 
  opacity: 0.6; 
  cursor: not-allowed; 
}

input, select { 
  width: 100%; 
  padding: 10px; 
  margin: 6px 0 14px; 
  font-size: 16px; 
  border: 1px solid #ccc;
  border-radius: 4px;
}

label { 
  font-weight: bold; 
  font-size: 14px;
}

table { 
  width: 100%; 
  border-collapse: collapse; 
  margin-top: 8px;
}

th, td { 
  text-align: left; 
  padding: 10px 8px; 
  border-bottom: 1px solid #eee; 
}

th { 
  background-color: #fafbfc; 
  font-weight: bold; 
}

.table-wrap { 
  overflow-x: auto; 
}

.error { 
  color: #b00020; 
  font-weight: bold;
}

.success { 
  color: #1b7f3b; 
  font-weight: bold;
}

.balance { 
  font-size: 32px; 
  font-weight: bold; 
  color: #1a3c6e;
  margin: 8px 0;
}
```

---

### File 2: Centralized API Client (`src/services/api.js`)
**EXPLAIN:**
- "Instead of scattering `fetch()` calls across every component, we centralize them into `api.js`. If the server address changes or returns an error, we handle it in one file."

**GO TO: VS Code**
1. Right-click `frontend/portal/src/services/`, select **New File**, name it `api.js`.
2. Paste:

```javascript
// Ensure this port matches the port printed by 'dotnet run'
const BASE_URL = "http://localhost:5123";

async function handle(response) {
  const data = await response.json().catch(() => ({}));
  if (!response.ok) {
    throw new Error(data.error || "Request failed");
  }
  return data;
}

export function getJson(path) {
  return fetch(`${BASE_URL}${path}`).then(handle);
}

export function postJson(path, body) {
  return fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  }).then(handle);
}
```

*(Note: Verify that `BASE_URL` matches your running backend port, e.g. 5123).*

---

### File 3: Authentication Context (`src/context/AuthContext.jsx`)
**EXPLAIN:**
- "React Context makes the logged-in user available everywhere in the app. Today we use three fixed training accounts: `adewale` (Taxpayer 101), `chioma` (Taxpayer 102), and `bisi` (LIRS Officer)."

**GO TO: VS Code**
1. Right-click `frontend/portal/src/context/`, select **New File**, name it `AuthContext.jsx`.
2. Paste:

```jsx
import { createContext, useContext, useState } from "react";

const AuthContext = createContext(null);

// Training users only. Real server login with password hashing is added on Day 4.
const USERS = [
  { username: "adewale", password: "pass123", role: "Taxpayer", taxpayerId: 101, name: "Adewale Ventures Ltd" },
  { username: "chioma",  password: "pass123", role: "Taxpayer", taxpayerId: 102, name: "Chioma Okafor" },
  { username: "bisi",    password: "pass123", role: "Officer",  taxpayerId: null, name: "Officer Bisi" },
];

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);

  function login(username, password) {
    const found = USERS.find((u) => u.username === username && u.password === password);
    if (!found) return null;
    setUser(found);
    return found;
  }

  function logout() {
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

### File 4: Currency Formatter Component (`src/components/Naira.jsx`)
**EXPLAIN:**
- "Instead of manually typing `₦` and decimal places everywhere, we create a reusable `<Naira value={...} />` component using JavaScript's official `en-NG` locale formatting."

**GO TO: VS Code**
1. Right-click `frontend/portal/src/components/`, select **New File**, name it `Naira.jsx`.
2. Paste:

```jsx
export default function Naira({ value }) {
  const amount = Number(value) || 0;
  return <>₦{amount.toLocaleString("en-NG", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</>;
}
```

---

### File 5: Portal Header (`src/components/TopBar.jsx`)
**EXPLAIN:**
- "Shows the agency name, the logged-in user's identity and role, and a one-click logout button."

**GO TO: VS Code**
1. Right-click `frontend/portal/src/components/`, select **New File**, name it `TopBar.jsx`.
2. Paste:

```jsx
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function TopBar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <div className="topbar">
      <strong>LIRS Taxpayer Portal</strong>
      {user && (
        <span>
          {user.name} ({user.role}){" "}
          <button onClick={handleLogout} style={{ marginLeft: "12px", padding: "6px 12px" }}>
            Log out
          </button>
        </span>
      )}
    </div>
  );
}
```

---

### File 6: Protected Route Guard (`src/components/ProtectedRoute.jsx`)
**EXPLAIN:**
- "A security guard in the browser: if an unauthenticated user tries to visit `/taxpayer`, it redirects them to `/login`. If a Taxpayer tries to visit `/officer`, it shows an access-denied message."

**GO TO: VS Code**
1. Right-click `frontend/portal/src/components/`, select **New File**, name it `ProtectedRoute.jsx`.
2. Paste:

```jsx
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function ProtectedRoute({ role, children }) {
  const { user } = useAuth();
  
  if (!user) {
    return <Navigate to="/login" replace />;
  }
  
  if (role && user.role !== role) {
    return (
      <div className="container">
        <p className="card error">You are not allowed to view this page. Required role: {role}.</p>
      </div>
    );
  }
  
  return children;
}
```

---

### File 7: Login Page (`src/pages/LoginPage.jsx`)
**EXPLAIN:**
- "Captures credentials and automatically navigates the user to their designated dashboard: `/taxpayer` for citizens, or `/officer` for LIRS staff."

**GO TO: VS Code**
1. Right-click `frontend/portal/src/pages/`, select **New File**, name it `LoginPage.jsx`.
2. Paste:

```jsx
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  function handleSubmit(event) {
    event.preventDefault();
    const user = login(username, password);
    if (!user) {
      setError("Invalid username or password.");
      return;
    }
    navigate(user.role === "Officer" ? "/officer" : "/taxpayer");
  }

  return (
    <div className="container" style={{ marginTop: "60px" }}>
      <div className="card" style={{ maxWidth: "440px", margin: "0 auto" }}>
        <h1 style={{ color: "#1a3c6e", marginTop: 0 }}>LIRS Portal Sign In</h1>
        <form onSubmit={handleSubmit}>
          <label htmlFor="username">Username</label>
          <input 
            id="username" 
            value={username} 
            onChange={(e) => setUsername(e.target.value)} 
            placeholder="e.g. adewale"
            required
          />

          <label htmlFor="password">Password</label>
          <input 
            id="password" 
            type="password" 
            value={password} 
            onChange={(e) => setPassword(e.target.value)} 
            placeholder="Enter password"
            required
          />

          {error && <p role="alert" className="error">{error}</p>}
          <button type="submit" style={{ width: "100%", marginTop: "8px" }}>Sign in</button>
        </form>
        <div style={{ marginTop: "20px", fontSize: "13px", color: "#666", borderTop: "1px solid #eee", paddingTop: "12px" }}>
          <strong>Training accounts:</strong><br />
          • Taxpayer: <code>adewale</code> / <code>pass123</code><br />
          • Taxpayer: <code>chioma</code> / <code>pass123</code><br />
          • LIRS Officer: <code>bisi</code> / <code>pass123</code>
        </div>
      </div>
    </div>
  );
}
```

---

### File 8: Taxpayer Dashboard (`src/pages/TaxpayerDashboard.jsx`)
**EXPLAIN:**
- "This is the primary citizen screen. When it opens, `useEffect` queries the API for both the balance and return history. When a payment is submitted, it validates inputs, posts to `/api/payments`, and increments `reload` so both the balance card and returns table refresh automatically!"

**GO TO: VS Code**
1. Right-click `frontend/portal/src/pages/`, select **New File**, name it `TaxpayerDashboard.jsx`.
2. Paste:

```jsx
import { useState, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { getJson, postJson } from "../services/api";
import TopBar from "../components/TopBar";
import Naira from "../components/Naira";

export default function TaxpayerDashboard() {
  const { user } = useAuth();
  const [balance, setBalance] = useState(null);
  const [returns, setReturns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [reload, setReload] = useState(0);

  const [returnId, setReturnId] = useState("");
  const [amount, setAmount] = useState("");
  const [channel, setChannel] = useState("Bank");
  const [message, setMessage] = useState("");
  const [formError, setFormError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        const b = await getJson(`/api/taxpayers/${user.taxpayerId}/balance`);
        const r = await getJson(`/api/taxpayers/${user.taxpayerId}/returns`);
        setBalance(b.balance);
        setReturns(r);
        setError("");
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [user.taxpayerId, reload]);

  async function handlePay(event) {
    event.preventDefault();
    setMessage("");
    setFormError("");

    if (!returnId) {
      setFormError("Please choose a return.");
      return;
    }
    if (Number(amount) <= 0) {
      setFormError("Amount must be above zero.");
      return;
    }

    try {
      setSubmitting(true);
      const result = await postJson("/api/payments", {
        returnId: Number(returnId),
        amount: Number(amount),
        channel,
      });
      setMessage(`Payment successful! Official Receipt: ${result.receiptNumber}.`);
      setAmount("");
      setReload((prev) => prev + 1); // Triggers re-fetch of balance & returns
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  }

  const payable = returns.filter((r) => r.status !== "Draft");

  return (
    <>
      <TopBar />
      <div className="container">
        {loading && <p>Loading taxpayer records from server...</p>}
        {error && <p role="alert" className="card error">{error}</p>}

        {!loading && !error && (
          <>
            <div className="card">
              <h2>Outstanding Liability</h2>
              <p className="balance"><Naira value={balance} /></p>
              {balance === 0 && <p className="success">✓ Your account is up to date.</p>}
            </div>

            <div className="card">
              <h2>My Tax Returns</h2>
              <div className="table-wrap">
                <table>
                  <thead>
                    <tr>
                      <th>Year</th>
                      <th>Declared Income</th>
                      <th>Tax Due</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {returns.map((r) => (
                      <tr key={r.returnId}>
                        <td>{r.taxYear}</td>
                        <td><Naira value={r.declaredIncome} /></td>
                        <td><Naira value={r.taxDue} /></td>
                        <td>
                          <span style={{ 
                            padding: "4px 8px", 
                            borderRadius: "4px", 
                            fontSize: "12px",
                            backgroundColor: r.status === "Approved" ? "#e6f4ea" : "#feefe3",
                            color: r.status === "Approved" ? "#137333" : "#b06000"
                          }}>
                            {r.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="card">
              <h2>Record a Tax Payment</h2>
              <form onSubmit={handlePay}>
                <label htmlFor="return">Select Tax Return</label>
                <select id="return" value={returnId} onChange={(e) => setReturnId(e.target.value)}>
                  <option value="">-- Choose a Return --</option>
                  {payable.map((r) => (
                    <option key={r.returnId} value={r.returnId}>
                      {r.taxYear} Return (Statutory Due: ₦{r.taxDue.toLocaleString()})
                    </option>
                  ))}
                </select>

                <label htmlFor="amount">Payment Amount (₦)</label>
                <input 
                  id="amount" 
                  type="number" 
                  step="0.01"
                  value={amount} 
                  onChange={(e) => setAmount(e.target.value)} 
                  placeholder="Enter amount in Naira"
                />

                <label htmlFor="channel">Payment Channel</label>
                <select id="channel" value={channel} onChange={(e) => setChannel(e.target.value)}>
                  <option value="Bank">Commercial Bank Branch</option>
                  <option value="Card">Debit Card / WebPAY</option>
                  <option value="USSD">Mobile USSD</option>
                </select>

                {formError && <p role="alert" className="error">{formError}</p>}
                {message && <p className="success">{message}</p>}
                
                <button type="submit" disabled={submitting}>
                  {submitting ? "Processing Payment..." : "Submit Payment"}
                </button>
              </form>
            </div>
          </>
        )}
      </div>
    </>
  );
}
```

---

### File 9: Officer Dashboard (`src/pages/OfficerDashboard.jsx`)
**EXPLAIN:**
- "The revenue officer screen. Allows searching all registered taxpayers or filtering by 10-digit TIN. Includes an on-demand 'Show' button to fetch balances live."

**GO TO: VS Code**
1. Right-click `frontend/portal/src/pages/`, select **New File**, name it `OfficerDashboard.jsx`.
2. Paste:

```jsx
import { useState } from "react";
import { getJson } from "../services/api";
import TopBar from "../components/TopBar";
import Naira from "../components/Naira";

export default function OfficerDashboard() {
  const [tin, setTin] = useState("");
  const [results, setResults] = useState(null);
  const [balances, setBalances] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSearch(event) {
    event.preventDefault();
    setError("");
    setLoading(true);
    try {
      const path = tin.trim() ? `/api/taxpayers?tin=${encodeURIComponent(tin.trim())}` : "/api/taxpayers";
      const data = await getJson(path);
      setResults(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function showBalance(id) {
    try {
      const data = await getJson(`/api/taxpayers/${id}/balance`);
      setBalances((prev) => ({ ...prev, [id]: data.balance }));
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <>
      <TopBar />
      <div className="container">
        <div className="card">
          <h2>Taxpayer Directory & Search</h2>
          <form onSubmit={handleSearch}>
            <label htmlFor="tin">Taxpayer Identification Number (TIN)</label>
            <div style={{ display: "flex", gap: "12px", alignItems: "center" }}>
              <input 
                id="tin" 
                value={tin} 
                onChange={(e) => setTin(e.target.value)} 
                placeholder="Enter 10-digit TIN (or leave blank to list all)"
                style={{ margin: 0 }}
              />
              <button type="submit" style={{ whiteSpace: "nowrap" }}>
                Search Directory
              </button>
            </div>
          </form>
        </div>

        {loading && <p>Searching state tax records...</p>}
        {error && <p role="alert" className="card error">{error}</p>}
        {results && results.length === 0 && <p className="card">No taxpayers found matching that search.</p>}

        {results && results.length > 0 && (
          <div className="card table-wrap">
            <h2>Search Results ({results.length} found)</h2>
            <table>
              <thead>
                <tr>
                  <th>TIN</th>
                  <th>Name</th>
                  <th>Type</th>
                  <th>State</th>
                  <th>Outstanding Balance</th>
                </tr>
              </thead>
              <tbody>
                {results.map((t) => (
                  <tr key={t.taxpayerId}>
                    <td><code>{t.tin || t.TIN}</code></td>
                    <td><strong>{t.name || t.Name}</strong></td>
                    <td>{t.type || t.Type}</td>
                    <td>{t.state || t.State}</td>
                    <td>
                      {balances[t.taxpayerId] !== undefined ? (
                        <strong><Naira value={balances[t.taxpayerId]} /></strong>
                      ) : (
                        <button 
                          onClick={() => showBalance(t.taxpayerId)}
                          style={{ padding: "4px 10px", fontSize: "12px" }}
                        >
                          Show
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </>
  );
}
```

---

### File 10: Application Routing (`src/App.jsx`)
**EXPLAIN:**
- "Defines the site map. Wraps all routes in `AuthProvider` so the entire tree has access to authentication. Uses `ProtectedRoute` to guard officer and taxpayer dashboards."

**GO TO: VS Code**
Open `frontend/portal/src/App.jsx` (replace its entire content):

```jsx
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import LoginPage from "./pages/LoginPage";
import TaxpayerDashboard from "./pages/TaxpayerDashboard";
import OfficerDashboard from "./pages/OfficerDashboard";

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/taxpayer"
            element={
              <ProtectedRoute role="Taxpayer">
                <TaxpayerDashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/officer"
            element={
              <ProtectedRoute role="Officer">
                <OfficerDashboard />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
```

---

### File 11: Application Entry Point (`src/main.jsx`)
**GO TO: VS Code**
Check `frontend/portal/src/main.jsx`. Ensure it cleanly renders `<App />` and imports `index.css`:

```jsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
```

*(If `main.jsx` imports `App.css`, delete that import line).*

---

## 7. PART FIVE: Testing the Complete Application (End-to-End)

With the backend API running on `http://localhost:5123` and the Vite frontend running on `http://localhost:5173`, walk through this comprehensive verification matrix with the participant.

**GO TO: Google Chrome or Microsoft Edge -> Open `http://localhost:5173`**

| # | Test Action | Exact Steps | Expected Result |
|---|---|---|---|
| **1** | Taxpayer Sign In | Type `adewale` / `pass123`, click **Sign in** | Navigates to `/taxpayer`. TopBar shows "Adewale Ventures Ltd (Taxpayer)". Outstanding Liability displays ₦700,000.00. Returns table lists 2025 and 2026. |
| **2** | Valid Payment Flow | In the payment form: select 2026 return, type `50000`, Channel `Commercial Bank Branch`, click **Submit Payment** | Green banner displays: `Payment successful! Official Receipt: RCT-000005.`. Outstanding balance immediately drops to ₦650,000.00! |
| **3** | Client Validation (Zero Amount) | Type `0` in amount, click **Submit Payment** | Red message displays: `Amount must be above zero.`. No network request is sent to the API. |
| **4** | Client Validation (No Return) | Clear the return dropdown, type `10000`, click **Submit Payment** | Red message displays: `Please choose a return.`. |
| **5** | Server Business Rule Validation (Overpayment) | Select 2026 return, type `1000000`, click **Submit Payment** | Red error from API: `Payment of 1,000,000.00 exceeds the outstanding 600,000.00.`. The application remains stable. |
| **6** | Officer Sign In | Click **Log out** in TopBar. Sign in as `bisi` / `pass123` | Navigates to `/officer`. TopBar shows "Officer Bisi (Officer)". Officer Directory screen displays. |
| **7** | Search by Specific TIN | Type `1000000001`, click **Search Directory** | Results table displays 1 row: Adewale Ventures Ltd, TIN `1000000001`, Lagos. |
| **8** | On-Demand Balance Lookup | Click **Show** button on Adewale's row | Button is replaced with the live balance: `₦650,000.00` (or `₦700,000.00` if reset). |
| **9** | List All Taxpayers | Clear TIN input box, click **Search Directory** | Results table displays all 4 taxpayers (Adewale, Chioma, Bello, Ngozi). |
| **10** | Role-Based Access Guard | Log out, log in as `adewale`. Manually type `http://localhost:5173/officer` into browser address bar | Screen displays red card: `You are not allowed to view this page. Required role: Officer.`. |
| **11** | Backend Resilience Test | In VS Code terminal Tab 1, press **Ctrl+C** to stop the backend API. In the browser, refresh the taxpayer dashboard | Screen displays clean red error card `Failed to fetch` or `Could not load taxpayers` instead of crashing. |

Restart the backend API (`dotnet run --launch-profile http`) after completing Test 11.

---

## 8. PART SIX: Save Work with Git and GitHub (Branches & PRs)

Follow the team Git workflow practiced on Day 1. We slice Day 3 into five logical feature branches and merge each using pull requests on GitHub.

**GO TO: VS Code terminal**
Open a terminal in the root repository directory:
```powershell
cd C:\Users\HP\Desktop\lirs\lirs-taxpayer-portal
```

### Slice 1: Enable CORS
```powershell
git checkout -b feature/cors
git add backend/LirsPortal.Api/Program.cs
git commit -m "feat: configure cors policy allowing react portal origin"
git push -u origin feature/cors
```
**GitHub steps:**
1. Open GitHub repo `lirs-taxpayer-portal`.
2. Click **Compare & pull request**.
3. Title: `Feature: CORS policy for React portal`.
4. Participant reviews and clicks **Merge pull request**, then **Confirm merge**.
5. Back in terminal:
```powershell
git checkout main
git pull origin main
```

### Slice 2: Vite React Setup & Styling
```powershell
git checkout -b feature/react-setup
git add frontend/
git commit -m "feat: scaffold vite react project with design system and api client"
git push -u origin feature/react-setup
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

### Slice 3: User Authentication & Routing (US-01)
```powershell
git checkout -b feature/US-01-login
git add frontend/portal/src/pages/LoginPage.jsx frontend/portal/src/components/ProtectedRoute.jsx frontend/portal/src/App.jsx
git commit -m "feat(US-01): implement login simulation, role guarding, and routing"
git push -u origin feature/US-01-login
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

### Slice 4: Taxpayer Dashboard & Payments (US-02, US-03, US-04)
```powershell
git checkout -b feature/US-02-US-03-US-04-taxpayer-dashboard
git add frontend/portal/src/pages/TaxpayerDashboard.jsx
git commit -m "feat(US-02,US-03,US-04): add taxpayer liability card, returns table, and payment flow"
git push -u origin feature/US-02-US-03-US-04-taxpayer-dashboard
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

### Slice 5: Officer Search & Directory (US-05)
```powershell
git checkout -b feature/US-05-officer-search
git add frontend/portal/src/pages/OfficerDashboard.jsx
git commit -m "feat(US-05): add revenue officer tin directory search and balance lookup"
git push -u origin feature/US-05-officer-search
```
*(Open PR on GitHub, review, merge to main, checkout main, git pull).*

---

## 9. If Time is Short: Priority Order & One-Branch Shortcut

If class discussions run long or npm package downloads take extra time, use this strict priority order:

| Priority | Feature | Why it matters |
|---|---|---|
| **1 (MUST DO)** | CORS in `Program.cs` | Without CORS, no frontend can ever communicate with the API. |
| **2 (MUST DO)** | Vite Setup & `api.js` | Scaffolds the React app and connects HTTP fetch calls. |
| **3 (MUST DO)** | Taxpayer Dashboard (`TaxpayerDashboard.jsx`) | Covers US-02 (Balance), US-03 (Returns), and US-04 (Payment). |
| **4 (HIGH)** | Login Simulation & `AuthContext` | Demonstrates role-based navigation and user identity. |
| **5 (MEDIUM)** | Officer Dashboard (`OfficerDashboard.jsx`) | Covers US-05 (TIN search). |
| **6 (STRETCH)** | Standalone `estimator.html` | Can be given as take-home study material. |

### The One-Branch Shortcut (if less than 30 minutes remain for Git):
Instead of 5 separate pull requests, commit all files in a single feature branch:
```powershell
git checkout -b feature/day3-complete-portal
git add .
git commit -m "feat: complete day 3 frontend portal with taxpayer and officer screens"
git push -u origin feature/day3-complete-portal
```
Open one single Pull Request on GitHub, conduct a mutual code review, and merge to `main`.

---

## 10. What the Participant Should Be Able to Say at the End

At 15:45, ask the participant to summarize today's work. He should comfortably express:

1. **On Frontend Architecture:** *"We build web frontends using modular React components instead of monolithic scripts. Components like `Naira` and `TopBar` are written once and reused everywhere."*
2. **On React State:** *"State drives the screen. When a taxpayer submits a payment, we update state, which automatically redraws the balance without needing to manually refresh the browser."*
3. **On Network Security & CORS:** *"Browsers block cross-origin calls by default. The backend must explicitly specify which frontend domains and ports are permitted to communicate via CORS."*
4. **On Client vs Server Security:** *"Front-end route guarding is for user experience and navigation. Real security must always be enforced on the API and database, which we tackle tomorrow on Day 4."*

---

## 11. Master Troubleshooting Table

| Problem / Error | Cause | Exact Fix |
|---|---|---|
| `Access to fetch at ... has been blocked by CORS policy` | Missing CORS in API or policy placed after `MapControllers` | In `Program.cs`, ensure `builder.Services.AddCors(...)` is added, and `app.UseCors("portal")` is placed **before** `app.MapControllers()`. Rebuild and restart API. |
| `Failed to fetch` in React console | Backend API is not running or wrong port in `api.js` | Check terminal running API. Verify port (e.g. 5123). In `frontend/portal/src/services/api.js`, update `BASE_URL` to match the exact port. |
| `Cannot find module 'react-router-dom'` | Package not installed in Vite project folder | Run `cd frontend/portal` then `npm install react-router-dom`. |
| Port 5173 already in use (Vite runs on 5174) | Another Vite process is still running | Stop the other terminal, or run `Get-Process -Name node | Stop-Process -Force`. |
| Officer table shows empty values for TIN | API sends lowercase property names (`tin` instead of `TIN`) | In `OfficerDashboard.jsx`, use `t.tin || t.TIN` and `t.name || t.Name`. |
| Payment submission does not update balance | Missing state refresh after POST | In `TaxpayerDashboard.jsx`, ensure `setReload(prev => prev + 1)` is called inside `handlePay` upon success. |
| `node_modules` showing up in `git status` | Missing `.gitignore` inside `portal/` or root | Ensure `.gitignore` in repo root contains `node_modules/`. Run `git rm -r --cached frontend/portal/node_modules` if tracked by mistake. |

---

## 12. Final Checklist

Before ending Day 3, verify:

- [ ] CORS is configured in `Program.cs` and the API runs on HTTP profile.
- [ ] Vite project starts cleanly with `npm run dev` at `http://localhost:5173`.
- [ ] Taxpayer `adewale` can sign in and see outstanding liability (₦700,000.00).
- [ ] Submitting a ₦50,000 payment creates an official receipt and drops the balance live.
- [ ] Entering zero or negative payment shows client validation error.
- [ ] Overpayment attempt triggers and displays the server's business rule rejection message.
- [ ] Officer `bisi` can sign in, search by TIN, and inspect live taxpayer balances.
- [ ] Non-officers are blocked from accessing `/officer`.
- [ ] All code is committed and merged into `main` via GitHub pull requests.
- [ ] System is clean and ready for Day 4 (Testing, Security, JWT, and Gateway Integration).

---

## 13. Quick Reference of Every Command Used Today

| Task | Command |
|---|---|
| Run backend API with HTTP profile | `dotnet run --launch-profile http` |
| Create Vite React application | `npm create vite@latest portal -- --template react` |
| Install React Router | `npm install react-router-dom` |
| Start frontend dev server | `npm run dev` |
| Kill hanging Node or .NET processes | `Get-Process -Name dotnet,node -ErrorAction SilentlyContinue | Stop-Process -Force` |
| Create Git feature branch | `git checkout -b feature/<branch-name>` |
| Push branch to GitHub | `git push -u origin feature/<branch-name>` |
| Pull merged changes from GitHub | `git checkout main; git pull origin main` |
