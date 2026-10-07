/**
 * ==============================================================================
 * Application Entry Point (src/main.jsx) - Day 3
 * ==============================================================================
 * Boots up the React 19 / 18 runtime, mounts the application into the root DOM node
 * defined in index.html, and applies the global CSS stylesheet.
 * ==============================================================================
 */

import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.jsx";

// Locate the mounting root div from index.html and render the application tree
createRoot(document.getElementById("root")).render(
  <StrictMode>
    <App />
  </StrictMode>
);
