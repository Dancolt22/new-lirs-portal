# Day 3: Frontend Development and User Experience

This day covers how screens are built: HTML for structure, CSS for appearance, JavaScript for behaviour, and then React, which combines all three into reusable pieces. It ends with the capstone's screens, login, and roles.

```
User -> Frontend (HTML, CSS, JavaScript, React) -> API -> Backend logic -> Database -> response back up
```

## 1. How a web page reaches the screen

**A browser** is a program that downloads web pages and draws them.
*Example:* Chrome, Edge, and Firefox.

**HTML, CSS, and JavaScript** are the three languages every web page is made from. HTML is the structure, CSS is the appearance, and JavaScript is the behaviour.
*Example:* A bank page. HTML says "here is a heading and a form". CSS makes the button green and the layout tidy. JavaScript checks the amount before sending it.

**A skeleton, paint, and electricity.** A building has walls (HTML), paint and furniture (CSS), and wiring that makes things work (JavaScript).
*Example:* A page with only HTML looks plain and stacked. Add CSS and it looks designed. Add JavaScript and the buttons do something.

**Developer Tools (DevTools)** are built-in browser tools for inspecting a page. Press **F12** to open them.
*Example:* The **Elements** tab shows the HTML and CSS of any page, the **Console** tab shows errors, and the **Network** tab shows every request the page sends to an API and the status code that came back.

## 2. HTML

**HTML (HyperText Markup Language)** describes the content and structure of a page using tags.
*Example:* `<h1>Welcome</h1>` tells the browser "this is the main heading". Search Engine Optimization(SEO)

**An element** is a tag pair with content between them. Most have an opening tag `<p>` and a closing tag `</p>`.
*Example:* `<p>Your balance is due on Friday.</p>`

**An attribute** adds extra information to an element, written inside the opening tag.
*Example:* `<a href="https://google.com">Open Google</a>`, where `href` is the address the link goes to.

**The basic page structure** every HTML file follows.
```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>My Page</title>
</head>
<body>
  <h1>Hello</h1>
</body>
</html>
```
*Example:* `<head>` holds information about the page (the tab title). `<body>` holds what the user sees. The `viewport` line makes the page fit phone screens.

**Headings and paragraphs.** `<h1>` to `<h6>` are headings from most to least important, and `<p>` is a paragraph.
*Example:* One `<h1>` for the page title, `<h2>` for each section, `<p>` for the text under it.

**Links and images.**
```html
<a href="contact.html">Contact us</a>
<img src="logo.png" alt="Company logo">
```
*Example:* The `alt` text is read aloud to people using screen readers and shown if the image fails to load.

**Lists.** `<ul>` is a bulleted list, `<ol>` is a numbered list, and `<li>` is one item.
```html
<ul>
  <li>Bank transfer</li>
  <li>Card</li>
  <li>USSD</li>
</ul>
```
*Example:* A list of payment options on a checkout page.

**Tables** show rows and columns of data. `<table>` holds `<tr>` rows, which hold `<th>` headings or `<td>` cells.
```html
<table>
  <thead>
    <tr>
      <th>Year</th>
      <th>Tax due</th>
      </tr>
      </thead>
  <tbody>
    <tr><td>2025</td><td>₦500,000</td></tr>
    <tr><td>2026</td><td>₦600,000</td></tr>
  </tbody>
</table>
```
*Example:* A list of a customer's invoices.

**Containers.** `<div>` is a generic block, and `<span>` is a generic inline piece of text.
*Example:* A `<div>` around a whole product card, and a `<span>` around only the price.

**Semantic elements** are tags that say what a section is for: `<header>`, `<nav>`, `<main>`, `<section>`, `<footer>`.
*Example:* `<nav>` wraps the menu, so assistive tools can jump straight to it.

**Forms** collect input from the user.
```html
<form>
  <label for="amount">Amount</label>
  <input id="amount" type="number" placeholder="Enter amount" required>

  <label for="channel">Channel</label>
  <select id="channel">
    <option>Bank</option>
    <option>Card</option>
    <option>USSD</option>
  </select>

  <button type="submit">Pay</button>
</form>
```
*Example:* A checkout form. `<label>` names the field, `<input>` is where the user types, `<select>` is a dropdown, and `<button>` submits.

**Input types** change how a field behaves: `text`, `number`, `email`, `password`, `date`, `checkbox`.
*Example:* `type="email"` on a phone brings up a keyboard with the `@` key, and `type="password"` hides the characters.

**Labels and accessibility.** Every input needs a `<label>` linked by `for` and `id`. Screen readers and tapping the label depend on it.
*Example:* Clicking the word "Amount" puts the cursor in the amount box.

## 3. CSS

**CSS (Cascading Style Sheets)** controls how HTML looks: colour, size, spacing, and layout.
*Example:* Making all headings dark blue and all buttons green.

**A rule** has a selector, which chooses elements, and declarations in braces that style them.
```css
h1 {
  color: #db5196;
  font-size: 28px;
}
```
*Example:* "Every `<h1>` on the page is dark blue and 28 pixels tall."

**Three ways to add CSS.** Inline in a tag, in a `<style>` block, or in a separate `.css` file linked from the page. The separate file is the standard for real projects.
```html
<link rel="stylesheet" href="styles.css">
```
*Example:* One `styles.css` file styles every page of a website, so a colour change happens in one place.

**Selectors** choose which elements a rule applies to.
- Element: `p { ... }` styles every paragraph.
- Class: `.card { ... }` styles every element with `class="card"`.
- ID: `#total { ... }` styles the one element with `id="total"`.
- Descendant: `.card p { ... }` styles paragraphs inside a card.
- State: `button:hover { ... }` styles a button while the mouse is over it.

*Example:* All product cards share the class `card`, and the checkout total has the id `total`.

**Common properties.**

| Property | What it controls | Example |
|---|---|---|
| `color` | Text colour | `color: #333;` |
| `background-color` | Background | `background-color: #f5f5f5;` |
| `font-family`, `font-size`, `font-weight` | Text | `font-size: 16px;` |
| `margin` | Space outside an element | `margin: 16px;` |
| `padding` | Space inside an element | `padding: 12px;` |
| `border` | Outline | `border: 1px solid #ccc;` |
| `border-radius` | Rounded corners | `border-radius: 8px;` |
| `width`, `height` | Size | `width: 100%;` |

**The box model** says every element is a box with content, padding, a border, and a margin, from the inside out.
*Example:* A card with 16px of padding keeps its text away from its border, and a margin keeps it away from the next card.

**`box-sizing: border-box`** makes width include padding and border, so sizes behave predictably.
```css
* { box-sizing: border-box; }
```
*Example:* A 300px-wide card stays 300px wide even after adding padding.

**Units.** `px` is a fixed pixel size. `%` is relative to the parent. `rem` is relative to the base font size. `vw` and `vh` are relative to the screen.
*Example:* Use `rem` for text so it scales with a user's settings, and `%` for widths so layouts stretch.

**Colours** can be names (`red`), hex codes (`#1a3c6e`), or `rgb(206, 36, 130)`.
*Example:* A brand colour is defined once and reused.

**Flexbox** lays items out in a row or column and spaces them evenly.
```css
.menu {
  display: flex;
  gap: 16px;
  justify-content: space-between;
  align-items: center;
}
```
*Example:* A navigation bar: logo on the left, links in the middle, a login button on the right.

**Grid** lays items out in rows and columns.
```css
.cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}
```
*Example:* A dashboard with three equal summary cards in a row.

**Responsive design** means a page adjusts to any screen size, using **media queries**.
```css
.cards { display: grid; grid-template-columns: 1fr; gap: 16px; }

@media (min-width: 768px) {
  .cards { grid-template-columns: repeat(3, 1fr); }
}
```
*Example:* Cards stack in one column on a phone and sit in three columns on a laptop.

**Mobile-first** means designing for the phone first, then adding rules for larger screens.
*Example:* Most visitors to a public service site use phones, so the phone layout is the default.

**A small stylesheet that ties it together:**
```css
* { box-sizing: border-box; }

body {
  font-family: Arial, sans-serif;
  margin: 0;
  background: #f5f7fa;
  color: #222;
}

.container { max-width: 900px; margin: 0 auto; padding: 16px; }

.card {
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 16px;
}

button {
  background: #1a3c6e;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 10px 16px;
  cursor: pointer;
}
button:hover { background: #12294d; }
```
*Example:* Every page that links this file gets the same look.

## 4. JavaScript

**JavaScript** is the programming language of the browser. It makes pages react to the user and talk to servers.
*Example:* Showing an error as soon as someone types an invalid amount, without reloading the page.

**Variables** store values. Use `const` for values that do not change, and `let` for values that do.
```js
const taxRate = 0.075;
let income = 5000000;
income = income + 100000;
```
*Example:* A tax rate is `const`, and a running total is `let`.

**Data types:** string (text), number, boolean (`true` or `false`), `null` and `undefined` (no value), array, and object.
```js
const name = "Chioma";        // string
const age = 32;               // number
const isPaid = false;         // boolean
```
*Example:* A customer's name is a string, their age a number, and whether they have paid a boolean.

**Operators** do arithmetic and comparison: `+ - * /`, `===` (equals), `!==` (not equals), `>`, `<`, `&&` (and), `||` (or).
*Example:* `balance > 0 && !isWaived` is true when someone owes money and has no waiver.

**Template literals** build text with values inside backticks.
```js
const message = `Hello ${name}, your balance is ₦${balance}`;
```
*Example:* A receipt line that includes the customer's name and amount.

**Conditions** choose what runs.
```js
if (balance > 0) {
  console.log("Payment due");
} else {
  console.log("Up to date");
}
```
*Example:* Showing "Payment due" or "You are up to date" depending on a balance.

**Loops** repeat work.
```js
for (let i = 1; i <= 3; i++) {
  console.log("Reminder", i);
}
```
*Example:* Sending three reminders in a row.

**Functions** are named, reusable blocks of code.
```js
function calculateTax(income, rate) {
  return income * rate;
}

const calculateTaxShort = (income, rate) => income * rate;   // arrow function
```
*Example:* One `calculateTax` function used on every page that shows a tax amount.

**Arrays** are ordered lists.
```js
const channels = ["Bank", "Card", "USSD"];
console.log(channels[0]);       // Bank
channels.push("Cash");
```
*Example:* A list of payment channels.

**Objects** group related values under names.
```js
const customer = { name: "Chioma", city: "Lagos", balance: 120000 };
console.log(customer.name);
```
*Example:* One customer record with name, city, and balance.

**Arrays of objects** are the usual shape of data from an API.
```js
const customers = [
  { id: 1, name: "Chioma", balance: 120000 },
  { id: 2, name: "Bello",  balance: 0 },
  { id: 3, name: "Ngozi",  balance: 300000 }
];
```
*Example:* A customer list returned by a server.

**Array methods** process lists without loops.

| Method | What it does | Example |
|---|---|---|
| `map` | Builds a new list by changing each item | `customers.map(c => c.name)` gives the names |
| `filter` | Keeps only matching items | `customers.filter(c => c.balance > 0)` gives those who owe |
| `find` | Returns the first match | `customers.find(c => c.id === 2)` |
| `reduce` | Combines everything into one value | `customers.reduce((sum, c) => sum + c.balance, 0)` gives the total owed |
| `forEach` | Runs code for each item | `customers.forEach(c => console.log(c.name))` |

*Example:* Total outstanding across all customers is one `reduce`.

**Destructuring** pulls values out of objects and arrays.
```js
const { name, balance } = customer;
```
*Example:* Taking `name` and `balance` out of a customer without writing `customer.` each time.

**The spread operator `...`** copies an object or array and lets you change part of it.
```js
const updated = { ...customer, balance: 0 };
```
*Example:* A new customer record with the balance set to zero, leaving the original untouched. React relies on this idea.

**The DOM (Document Object Model)** is the browser's live version of the page that JavaScript can read and change.
*Example:* Changing the text of a heading, hiding a message, or adding a row to a table.

**Selecting and changing elements.**
```js
const title = document.querySelector("h1");
title.textContent = "Welcome back";

const button = document.querySelector("#payButton");
button.disabled = true;
```
*Example:* Disabling the pay button after it is clicked, so a payment is not sent twice.

**Events** are things the user does: click, type, submit.
```js
button.addEventListener("click", () => {
  console.log("Clicked");
});
```
*Example:* Running a calculation when someone clicks "Calculate".

**Handling a form.**
```js
const form = document.querySelector("form");
form.addEventListener("submit", (event) => {
  event.preventDefault();                     // stop the page reloading
  const amount = Number(document.querySelector("#amount").value);
  console.log("Paying", amount);
});
```
*Example:* Reading what a user typed and checking it before sending. `preventDefault()` stops the browser's normal behaviour of reloading the page.

**JSON** is a text format for data that JavaScript can convert to and from objects.
```js
const text = '{"name":"Chioma","balance":120000}';
const obj = JSON.parse(text);
const back = JSON.stringify(obj);
```
*Example:* Data sent between a browser and a server travels as JSON text.

**`fetch`** sends a request to an API and waits for the answer.
```js
const response = await fetch("http://localhost:5123/api/taxpayers");
const data = await response.json();
```
*Example:* Loading a customer list from the server.

**`async` and `await`** let code wait for something slow (a network request) without freezing the page.
```js
async function loadTaxpayers() {
  const response = await fetch("http://localhost:5123/api/taxpayers");
  return await response.json();
}
```
*Example:* A page shows "Loading..." while the data arrives, then shows the list.

**Checking the response and handling errors.**
```js
async function loadTaxpayers() {
  try {
    const response = await fetch("http://localhost:5123/api/taxpayers");
    if (!response.ok) throw new Error("Could not load taxpayers");
    return await response.json();
  } catch (error) {
    console.log(error.message);
    return [];
  }
}
```
*Example:* If the server is down, the page shows a friendly message instead of breaking. `response.ok` is false for 400, 404, and 500 answers.

**A tiny page that uses HTML, CSS, and JavaScript together.** Save as `estimator.html` and open it in a browser.
```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Tax Estimator</title>
  <style>
    body { font-family: Arial, sans-serif; background: #f5f7fa; margin: 0; }
    .card { max-width: 400px; margin: 40px auto; background: white; padding: 20px;
            border-radius: 8px; border: 1px solid #ddd; }
    input, button { width: 100%; padding: 10px; margin-top: 8px; font-size: 16px; }
    button { background: #1a3c6e; color: white; border: none; border-radius: 6px; cursor: pointer; }
    #result { margin-top: 16px; font-weight: bold; }
    .error { color: #b00020; }
  </style>
</head>
<body>
  <div class="card">
    <h1>Tax Estimator</h1>
    <label for="income">Annual income (₦)</label>
    <input id="income" type="number" placeholder="e.g. 5000000">
    <button id="calc">Calculate 10% tax</button>
    <p id="result"></p>
  </div>

  <script>
    const button = document.querySelector("#calc");
    const result = document.querySelector("#result");

    button.addEventListener("click", () => {
      const income = Number(document.querySelector("#income").value);

      if (income <= 0) {
        result.textContent = "Please enter an income above zero.";
        result.className = "error";
        return;
      }

      const tax = income * 0.10;
      result.textContent = `Estimated tax: ₦${tax.toLocaleString()}`;
      result.className = "";
    });
  </script>
</body>
</html>
```
*Example:* The HTML gives the form, the CSS styles the card, and the JavaScript reads the input, validates it, and writes the answer back into the page.

**Modules** let JavaScript be split across files with `export` and `import`.
```js
// tax.js
export function calculateTax(income, rate) { return income * rate; }

// app.js
import { calculateTax } from "./tax.js";
```
*Example:* One file for tax rules, one for the screen. React projects are built from many small module files.

## 5. Node.js and npm

**Node.js** runs JavaScript outside the browser, on your computer. Development tools run on it.
*Example:* The tool that starts a React project and serves it locally is a Node program.

**npm** is the package manager that downloads libraries and runs project commands.
```bash
node -v
npm -v
npm install react-router-dom
npm run dev
```
*Example:* `npm install` downloads everything a project needs. `npm run dev` starts it.

**`package.json`** is a file listing a project's libraries and commands.
*Example:* It records that the project uses React, so a colleague can run `npm install` and get the same set.

**`node_modules`** is the folder where downloaded libraries are stored. It is large and never goes into Git.
*Example:* It is listed in `.gitignore`.

## 6. React

**React** is a JavaScript library for building user interfaces out of small reusable pieces called components.
*Example:* A shopping site is built from a `ProductCard`, a `Cart`, and a `SearchBar`, each written once and reused.

**Why React.** Plain JavaScript must find and update page elements by hand. React updates the page for you whenever your data changes.
*Example:* When a payment is saved, you change the balance value and React redraws every place that shows it.

**Vite** is a tool that creates and runs a React project quickly.
```bash
npm create vite@latest portal -- --template react
cd portal
npm install
npm run dev
```
*Example:* After `npm run dev`, the terminal prints an address such as `http://localhost:5173`. Opening it shows the app, which updates live as you save files.

**A component** is a JavaScript function that returns what to show.
```jsx
function WelcomeBanner() {
  return <h1>Welcome to the portal</h1>;
}
```
*Example:* `WelcomeBanner` can be placed on any page as `<WelcomeBanner />`.

**JSX** is the HTML-like syntax inside JavaScript. Three rules: return one parent element, use `className` instead of `class`, and put JavaScript inside `{}`.
```jsx
function Greeting() {
  const name = "Chioma";
  return (
    <div className="card">
      <h2>Hello {name}</h2>
      <p>Today is {new Date().toDateString()}</p>
    </div>
  );
}
```
*Example:* `{name}` inserts the value of the variable into the page.

**Props** are inputs passed from a parent component to a child, like function arguments. They are read-only.
```jsx
function ProductCard({ name, price }) {
  return (
    <div className="card">
      <h3>{name}</h3>
      <p>₦{price.toLocaleString()}</p>
    </div>
  );
}

function App() {
  return (
    <div>
      <ProductCard name="Rice 50kg" price={78000} />
      <ProductCard name="Beans 25kg" price={52000} />
    </div>
  );
}
```
*Example:* One `ProductCard`, two different products.

**State** is data a component remembers, and when it changes, React redraws the component. It is created with `useState`.
```jsx
import { useState } from "react";

function Counter() {
  const [count, setCount] = useState(0);

  return (
    <div>
      <p>Clicked {count} times</p>
      <button onClick={() => setCount(count + 1)}>Add one</button>
    </div>
  );
}
```
*Example:* `count` holds the value and `setCount` changes it. Never write `count = 5` directly. Always use the setter, because the setter tells React to redraw.

**Events** are handled with props such as `onClick`, `onChange`, and `onSubmit`.
*Example:* `<button onClick={handlePay}>Pay</button>` runs `handlePay` when clicked.

**Conditional rendering** shows different things depending on data.
```jsx
{balance > 0 ? <p>Outstanding: ₦{balance}</p> : <p>You are up to date</p>}
{isAdmin && <button>Delete account</button>}
```
*Example:* An admin sees a delete button and other users do not.

**Lists and keys** show many items with `map`. Each item needs a unique `key`.
```jsx
function InvoiceList({ invoices }) {
  return (
    <ul>
      {invoices.map((inv) => (
        <li key={inv.id}>{inv.year}: ₦{inv.amount.toLocaleString()}</li>
      ))}
    </ul>
  );
}
```
*Example:* A list of invoices from a server. `key` lets React track which item is which when the list changes.

**Forms with controlled inputs.** React keeps the input's value in state.
```jsx
import { useState } from "react";

function PaymentForm({ onSubmit }) {
  const [amount, setAmount] = useState("");
  const [channel, setChannel] = useState("Bank");
  const [error, setError] = useState("");

  function handleSubmit(event) {
    event.preventDefault();
    if (Number(amount) <= 0) {
      setError("Amount must be above zero.");
      return;
    }
    setError("");
    onSubmit({ amount: Number(amount), channel });
  }

  return (
    <form onSubmit={handleSubmit}>
      <label htmlFor="amount">Amount (₦)</label>
      <input id="amount" type="number" value={amount} onChange={(e) => setAmount(e.target.value)} />

      <label htmlFor="channel">Channel</label>
      <select id="channel" value={channel} onChange={(e) => setChannel(e.target.value)}>
        <option>Bank</option>
        <option>Card</option>
        <option>USSD</option>
      </select>

      {error && <p role="alert" className="error">{error}</p>}
      <button type="submit">Pay</button>
    </form>
  );
}
```
*Example:* A checkout form that checks the amount before passing it up. In JSX, `for` is written `htmlFor`.

**`useEffect`** runs code after the component appears on screen. It is where data is fetched.
```jsx
import { useState, useEffect } from "react";

function CustomerList() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function load() {
      try {
        const response = await fetch("http://localhost:5123/api/taxpayers");
        if (!response.ok) throw new Error("Could not load customers");
        setCustomers(await response.json());
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, []);

  if (loading) return <p>Loading...</p>;
  if (error) return <p role="alert">{error}</p>;
  return <ul>{customers.map((c) => <li key={c.taxpayerId}>{c.name}</li>)}</ul>;
}
```
*Example:* The empty `[]` at the end means "run once, when the page first appears". Leaving it out causes an endless loop of requests.

**Loading, error, and empty states.** Every screen that loads data handles all of them: still loading, failed, and nothing found.
*Example:* On a slow mobile connection, a user sees "Loading...", then either the list, "No records found", or "Could not load. Try again."

**Reusable components and folders.** Small components used in many places go in a `components` folder.
```
src/
  components/    Card, Button, Table, Loader, ErrorMessage
  pages/         LoginPage, Dashboard
  services/      api.js  (all server calls)
  context/       AuthContext.jsx
  App.jsx
  main.jsx
```
*Example:* A `Loader` written once shows "Loading..." everywhere.

**Lifting state up** means moving state to the nearest parent when two components need the same data.
*Example:* A search box and a results list both need the search text, so the parent holds it and passes it down as props.

**Context** shares data with many components without passing props through every level.
*Example:* The logged-in user is needed by the menu, the dashboard, and the header, so it lives in a context.

**Routing** shows different pages for different addresses without reloading the site. React Router is the standard library.
```jsx
import { BrowserRouter, Routes, Route, Link } from "react-router-dom";

function App() {
  return (
    <BrowserRouter>
      <nav><Link to="/">Home</Link> <Link to="/about">About</Link></nav>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
        <Route path="/customers/:id" element={<CustomerDetails />} />
      </Routes>
    </BrowserRouter>
  );
}
```
*Example:* `/customers/101` opens the details page for customer 101, and `:id` carries the number into the page.

**Frontend and backend communication** follows one path every time.
*Example:* The user clicks Pay, a handler runs, `fetch` sends `POST /api/payments`, the API checks and saves, JSON comes back, the component sets state, and the screen shows the receipt.

**CORS** is a browser safety rule that blocks a page from calling an API on a different address unless the API allows it.
*Example:* A React app on `localhost:5173` calling an API on `localhost:5123` is blocked until the API is told to allow `localhost:5173`.

**Blazor, briefly.** Blazor is Microsoft's framework for building web pages with C# instead of JavaScript, inside the .NET ecosystem. It also uses components. Where React runs in the browser and is written in JavaScript, Blazor components are written in C# in `.razor` files, running on the server (Blazor Server) or in the browser through WebAssembly (Blazor WebAssembly).
*Example:* A .NET-only team that wants one language for backend and frontend might choose Blazor. React has a far larger ecosystem and job market, so it is the focus here.

## 7. User experience and accessibility

**UX (user experience)** is how a product feels to use. **UI (user interface)** is how it looks.
*Example:* A page can look attractive (UI) but be frustrating if it takes six clicks to pay a bill (UX).

**Simplicity.** One main action per screen, in plain language.
*Example:* A button labelled "Pay now", not "Initiate remittance".

**Clear navigation.** The user always knows where they are and how to go back.
*Example:* A menu on every page and a visible "Back to dashboard" link.

**Forms.** Label every field, group related fields, mark required ones, and keep forms short.
*Example:* A payment form asks only for the return, amount, and channel.

**Feedback.** The system always tells the user what is happening.
*Example:* "Payment received. Receipt RCT-000005." after success, and a spinner while waiting.

**Error messages** say what went wrong and what to do next.
*Example:* "Amount must be above zero." is helpful. "Error 400" is not.

**Dashboards** put the most important thing first and avoid clutter.
*Example:* The balance due at the top, recent payments underneath.

**Tables** need sorting, readable columns, and a plan for small screens.
*Example:* On a phone, a wide table scrolls sideways inside its own box instead of breaking the page.

**Accessibility (a11y)** means a product works for everyone, including people with visual, hearing, or motor difficulties, and on poor connections.
*Example:* A visually impaired customer uses a screen reader to pay a bill.

**Accessibility checklist**
- Use real elements: `<button>` for buttons, `<label>` for fields, headings in order.
- Give every image an `alt` description.
- Use enough colour contrast, and never show an error by colour alone.
- Make everything usable with the keyboard (Tab, Enter).
- Let text be enlarged without the layout breaking.

*Example:* An error that is red **and** has the words "Amount is required" works for colour-blind users too.

## 8. Authentication, authorization, and roles

**Authentication** is proving who you are.
*Example:* Entering a username and password, or a one-time code sent by SMS.

**Authorization** is deciding what you are allowed to do once your identity is known.
*Example:* A bank customer can view their own account, but cannot approve loans.

**The gate and the tag.** At an office, the security desk checks your ID (authentication), and your visitor tag decides which floors you can enter (authorization).
*Example:* Everyone passes the gate, but only staff tags open the server room.

**A role** is a named group of permissions. **A permission** is one allowed action.
*Example:* The role Officer includes the permissions "view any taxpayer" and "approve a return".

**RBAC (Role-Based Access Control)** gives permissions to roles, and roles to users.
*Example:* A new officer is given the Officer role and automatically gets every officer permission.

| Role | Allowed |
|---|---|
| Taxpayer | View own records, submit returns, make payments |
| LIRS Officer | View any taxpayer, review submissions, run reports |
| Admin | Manage users and roles |

**How login works (token flow).**
1. The user sends a username and password to the server.
2. The server checks them. Passwords are stored **hashed**, never as plain text.
3. The server returns a **token**, a signed string that says who the user is and their role.
4. The browser sends the token with every later request.
5. The server checks the token and the role before doing anything.

*Example:* After login, every request carries `Authorization: Bearer <token>`.

**Frontend checks versus backend checks.** Hiding a button in React is not security. Anyone can call the API directly. The backend must check the role on every endpoint. The frontend check only improves the experience.
*Example:* An officer-only report must refuse a taxpayer's token even if the taxpayer types the address into Postman.

**A protected route** is a page that only logged-in users with the right role can open.
*Example:* Opening the officer dashboard without being logged in sends you to the login page.

## Capstone work for Day 3

By the end of Day 3 the capstone has screens: a login page, a taxpayer dashboard (balance, returns, payment form), and an officer dashboard (search by TIN). The login today is a **front-end simulation** with fixed training users. Real authentication on the server is added on Day 4, so the role checks shown here are for the experience only.

### Step 1: allow the frontend to call the API (CORS)

The API from Day 2 must be told to accept calls from the React app. In `backend/LirsPortal.Api/Program.cs`, add these two things.

After `builder.Services.AddControllers();` add:
```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("portal", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));
```

Before `app.MapControllers();` add:
```csharp
app.UseCors("portal");
```
Stop the API with Ctrl+C and run `dotnet run --launch-profile http` again. Note the port it prints.

### Step 2: create the React project

In a terminal, from the project folder:
```bash
cd frontend
npm create vite@latest portal -- --template react
cd portal
npm install
npm install react-router-dom
npm run dev
```
If the tool asks questions, choose **React** and **JavaScript**. Open the address it prints (usually `http://localhost:5173`). Leave this running, and use a second terminal for anything else.

In `src`, delete `App.css` and the `assets` folder, and create folders `components`, `pages`, `services`, and `context`.

### Step 3: the styles, `src/index.css` (replace everything)

```css
* { box-sizing: border-box; }
body { font-family: Arial, sans-serif; margin: 0; background: #f5f7fa; color: #222; }
.container { max-width: 900px; margin: 0 auto; padding: 16px; }
.card { background: white; border: 1px solid #ddd; border-radius: 8px; padding: 16px; margin-bottom: 16px; }
.topbar { display: flex; justify-content: space-between; align-items: center; background: #1a3c6e; color: white; padding: 12px 16px; }
button { background: #1a3c6e; color: white; border: none; border-radius: 6px; padding: 10px 16px; cursor: pointer; }
button:disabled { opacity: 0.6; cursor: not-allowed; }
input, select { width: 100%; padding: 10px; margin: 6px 0 12px; font-size: 16px; }
label { font-weight: bold; }
table { width: 100%; border-collapse: collapse; }
th, td { text-align: left; padding: 8px; border-bottom: 1px solid #eee; }
.table-wrap { overflow-x: auto; }
.error { color: #b00020; }
.success { color: #1b7f3b; }
.balance { font-size: 28px; font-weight: bold; }
```

### Step 4: the API helper, `src/services/api.js`

```js
// Use the port printed by 'dotnet run' for your API
const BASE_URL = "http://localhost:5123";

async function handle(response) {
  const data = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(data.error || "Request failed");
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

### Step 5: the login context, `src/context/AuthContext.jsx`

```jsx
import { createContext, useContext, useState } from "react";

const AuthContext = createContext(null);

// Training users only. Real login with a server is added on Day 4.
const USERS = [
  { username: "adewale", password: "pass123", role: "Taxpayer", taxpayerId: 101, name: "Adewale Ventures Ltd" },
  { username: "chioma",  password: "pass123", role: "Taxpayer", taxpayerId: 102, name: "Chioma Okafor" },
  { username: "bisi",    password: "pass123", role: "Officer",  name: "Officer Bisi" },
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

  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}
```

### Step 6: shared small components

`src/components/ProtectedRoute.jsx`
```jsx
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function ProtectedRoute({ role, children }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" />;
  if (role && user.role !== role) return <p className="container error">You are not allowed to view this page.</p>;
  return children;
}
```

`src/components/TopBar.jsx`
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
      <span>
        {user.name} ({user.role}) <button onClick={handleLogout}>Log out</button>
      </span>
    </div>
  );
}
```

`src/components/Naira.jsx`
```jsx
export default function Naira({ value }) {
  return <>₦{Number(value).toLocaleString("en-NG", { minimumFractionDigits: 2 })}</>;
}
```

### Step 7: the login page, `src/pages/LoginPage.jsx`

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
    <div className="container">
      <div className="card">
        <h1>Sign in</h1>
        <form onSubmit={handleSubmit}>
          <label htmlFor="username">Username</label>
          <input id="username" value={username} onChange={(e) => setUsername(e.target.value)} />

          <label htmlFor="password">Password</label>
          <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />

          {error && <p role="alert" className="error">{error}</p>}
          <button type="submit">Sign in</button>
        </form>
        <p>Training users: adewale, chioma, or bisi. Password: pass123</p>
      </div>
    </div>
  );
}
```

### Step 8: the taxpayer dashboard, `src/pages/TaxpayerDashboard.jsx`

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
    if (!returnId) return setFormError("Please choose a return.");
    if (Number(amount) <= 0) return setFormError("Amount must be above zero.");

    try {
      const result = await postJson("/api/payments", {
        returnId: Number(returnId),
        amount: Number(amount),
        channel,
      });
      setMessage(`Payment received. Receipt ${result.receiptNumber}.`);
      setAmount("");
      setReload(reload + 1);
    } catch (err) {
      setFormError(err.message);
    }
  }

  const payable = returns.filter((r) => r.status !== "Draft");

  return (
    <>
      <TopBar />
      <div className="container">
        {loading && <p>Loading...</p>}
        {error && <p role="alert" className="error">{error}</p>}

        {!loading && !error && (
          <>
            <div className="card">
              <h2>Outstanding balance</h2>
              <p className="balance"><Naira value={balance} /></p>
              {balance === 0 && <p className="success">You are up to date.</p>}
            </div>

            <div className="card">
              <h2>My tax returns</h2>
              <div className="table-wrap">
                <table>
                  <thead>
                    <tr><th>Year</th><th>Income</th><th>Tax due</th><th>Status</th></tr>
                  </thead>
                  <tbody>
                    {returns.map((r) => (
                      <tr key={r.returnId}>
                        <td>{r.taxYear}</td>
                        <td><Naira value={r.declaredIncome} /></td>
                        <td><Naira value={r.taxDue} /></td>
                        <td>{r.status}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="card">
              <h2>Make a payment</h2>
              <form onSubmit={handlePay}>
                <label htmlFor="return">Return</label>
                <select id="return" value={returnId} onChange={(e) => setReturnId(e.target.value)}>
                  <option value="">Choose a return</option>
                  {payable.map((r) => (
                    <option key={r.returnId} value={r.returnId}>
                      {r.taxYear} (tax due {r.taxDue.toLocaleString()})
                    </option>
                  ))}
                </select>

                <label htmlFor="amount">Amount (₦)</label>
                <input id="amount" type="number" value={amount} onChange={(e) => setAmount(e.target.value)} />

                <label htmlFor="channel">Channel</label>
                <select id="channel" value={channel} onChange={(e) => setChannel(e.target.value)}>
                  <option>Bank</option>
                  <option>Card</option>
                  <option>USSD</option>
                </select>

                {formError && <p role="alert" className="error">{formError}</p>}
                {message && <p className="success">{message}</p>}
                <button type="submit">Pay</button>
              </form>
            </div>
          </>
        )}
      </div>
    </>
  );
}
```

### Step 9: the officer dashboard, `src/pages/OfficerDashboard.jsx`

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
      const path = tin ? `/api/taxpayers?tin=${encodeURIComponent(tin)}` : "/api/taxpayers";
      setResults(await getJson(path));
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  async function showBalance(id) {
    try {
      const data = await getJson(`/api/taxpayers/${id}/balance`);
      setBalances({ ...balances, [id]: data.balance });
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <>
      <TopBar />
      <div className="container">
        <div className="card">
          <h2>Find a taxpayer</h2>
          <form onSubmit={handleSearch}>
            <label htmlFor="tin">TIN (leave empty to list all)</label>
            <input id="tin" value={tin} onChange={(e) => setTin(e.target.value)} />
            <button type="submit">Search</button>
          </form>
        </div>

        {loading && <p>Loading...</p>}
        {error && <p role="alert" className="error">{error}</p>}
        {results && results.length === 0 && <p>No taxpayer found.</p>}

        {results && results.length > 0 && (
          <div className="card table-wrap">
            <table>
              <thead>
                <tr><th>TIN</th><th>Name</th><th>State</th><th>Balance</th></tr>
              </thead>
              <tbody>
                {results.map((t) => (
                  <tr key={t.taxpayerId}>
                    <td>{t.tin}</td>
                    <td>{t.name}</td>
                    <td>{t.state}</td>
                    <td>
                      {balances[t.taxpayerId] !== undefined
                        ? <Naira value={balances[t.taxpayerId]} />
                        : <button onClick={() => showBalance(t.taxpayerId)}>Show</button>}
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

### Step 10: connect everything, `src/App.jsx` (replace everything)

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
            element={<ProtectedRoute role="Taxpayer"><TaxpayerDashboard /></ProtectedRoute>}
          />
          <Route
            path="/officer"
            element={<ProtectedRoute role="Officer"><OfficerDashboard /></ProtectedRoute>}
          />
          <Route path="*" element={<Navigate to="/login" />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
```

### Step 11: try it

With the API running and `npm run dev` running, open `http://localhost:5173`.

| Try | Expected |
|---|---|
| Sign in as `adewale` / `pass123` | Taxpayer dashboard, balance ₦700,000.00 (it will be lower if payments were made on Day 2), two returns listed |
| Pay ₦50,000 on the 2026 return | Green receipt message, balance drops by ₦50,000 |
| Pay `0` or leave the return empty | Red error message, nothing sent |
| Pay more than is owed | Red message from the API: payment exceeds the outstanding amount |
| Log out, sign in as `bisi` / `pass123` | Officer dashboard |
| Search TIN `1000000001` | One row, Adewale Ventures Ltd |
| Click **Show** | That taxpayer's balance appears |
| As `adewale`, type `/officer` in the address bar | "You are not allowed to view this page." |
| Stop the API and refresh the taxpayer page | A red error instead of a broken page |

### Git work for Day 3

| Branch | Contents |
|---|---|
| `feature/cors` | The two lines added to `Program.cs` |
| `feature/react-setup` | The Vite project, `index.css`, `api.js`, `AuthContext.jsx`, shared components |
| `feature/US-01-login` | `LoginPage.jsx`, `ProtectedRoute.jsx`, `App.jsx` routing |
| `feature/US-02-US-03-US-04-taxpayer-dashboard` | `TaxpayerDashboard.jsx` |
| `feature/US-05-officer-search` | `OfficerDashboard.jsx` |

The `.gitignore` already excludes `node_modules/`, so the libraries stay out of Git.

### State of the capstone at the end of Day 3

| Deliverable | Status |
|---|---|
| React project running with routing | Complete |
| Login page with two roles (front-end simulation) | Complete |
| Taxpayer dashboard: balance, returns, payment form | Complete |
| Officer dashboard: TIN search and balances | Complete |
| Loading, error, and success states on every screen | Complete |
| Mobile-friendly layout | Complete |
| Server-side login, password hashing, and role checks on the API | Day 4 |
| Tests and security fixes | Day 4 |
