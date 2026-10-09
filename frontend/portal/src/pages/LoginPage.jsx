/**
 * ==============================================================================
 * Portal Sign In Page (src/pages/LoginPage.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * Controlled React form that captures user credentials, authenticates via AuthContext,
 * and automatically directs users to their appropriate dashboard:
 * - Officer -> `/officer`
 * - Taxpayer -> `/taxpayer`
 * ==============================================================================
 */

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  // Controlled component state for form inputs and error messages
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  /**
   * Handles sign-in submission:
   * 1. Prevents default browser form submission refresh
   * 2. Calls login() from AuthContext
   * 3. Redirects based on user role if credentials match
   */
  async function handleSubmit(event) {
    event.preventDefault();
    
    // Clear previous error
    setError("");

    try {
      const authenticatedUser = await login(username.trim(), password);
      
      if (!authenticatedUser) {
        setError("Invalid username or password.");
        return;
      }

      // Role-based landing page redirection
      navigate(authenticatedUser.role === "Officer" ? "/officer" : "/taxpayer");
    } catch (err) {
      setError(err.message || "Invalid username or password.");
    }
  }

  return (
    <div className="container" style={{ marginTop: "60px" }}>
      <div className="card" style={{ maxWidth: "440px", margin: "0 auto" }}>
        <h1 style={{ color: "#1a3c6e", marginTop: 0 }}>LIRS Portal Sign In</h1>
        
        <form onSubmit={handleSubmit}>
          {/* Username Input Field */}
          <label htmlFor="username">Username</label>
          <input 
            id="username" 
            value={username} 
            onChange={(e) => setUsername(e.target.value)} 
            placeholder="e.g. adewale"
            required
            autoComplete="username"
          />

          {/* Password Input Field */}
          <label htmlFor="password">Password</label>
          <input 
            id="password" 
            type="password" 
            value={password} 
            onChange={(e) => setPassword(e.target.value)} 
            placeholder="Enter password"
            required
            autoComplete="current-password"
          />

          {/* Conditional Error Display */}
          {error && <p role="alert" className="error">{error}</p>}
          
          <button type="submit" style={{ width: "100%", marginTop: "8px" }}>
            Sign in
          </button>
        </form>

        {/* Training Helper Note */}
        <div style={{ marginTop: "20px", fontSize: "13px", color: "#666", borderTop: "1px solid #eee", paddingTop: "12px", lineHeight: "1.6" }}>
          <strong>Training accounts (Password for all: <code>pass123</code>):</strong><br />
          • <strong>Taxpayers:</strong> <code>adewale</code>, <code>chioma</code>, <code>bello</code>, <code>ngozi</code>, <code>emeka</code>, <code>fatima</code>, <code>tunde</code>, <code>amaka</code>, <code>ibrahim</code>, <code>kemi</code>, <code>olumide</code>, <code>zainab</code><br />
          • <strong>Officers:</strong> <code>bisi</code>, <code>folake</code>
        </div>
      </div>
    </div>
  );
}
