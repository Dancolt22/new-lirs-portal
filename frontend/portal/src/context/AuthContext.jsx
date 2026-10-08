/**
 * ==============================================================================
 * Authentication Context & State Provider (src/context/AuthContext.jsx) - Day 4
 * ==============================================================================
 * Architecture & Security Principles:
 * 1. Server-Side Cryptographic Authentication:
 *    Replaces in-memory mock logins with real HTTP calls to `POST /api/auth/login`.
 *    The server verifies PBKDF2 salted password hashes, enforces lockout penalties,
 *    and issues a signed JWT token.
 * 2. Session Persistence:
 *    Persists the issued JWT token (`lirs_token`) and sanitized user profile (`lirs_user`)
 *    in browser `sessionStorage`. This ensures that refreshing the browser tab retains
 *    the authenticated state without requiring re-login, while closing the browser tab
 *    automatically clears the credentials for security.
 * ==============================================================================
 */

import { createContext, useContext, useState } from "react";
import { postJson } from "../services/api";

const AuthContext = createContext(null);

/**
 * AuthProvider wraps the application and exposes user state, login, and logout.
 *
 * @param {object} props
 * @param {React.ReactNode} props.children - Child components that require auth access
 */
export function AuthProvider({ children }) {
  // Initialize state from sessionStorage if a prior session exists
  const [user, setUser] = useState(() => {
    const saved = sessionStorage.getItem("lirs_user");
    return saved ? JSON.parse(saved) : null;
  });

  /**
   * Submits credentials to the backend API, stores the signed JWT token,
   * and updates global user state.
   *
   * @param {string} username - User login name (e.g. adewale, chioma, bisi)
   * @param {string} password - User plaintext password (e.g. pass123)
   * @returns {Promise<object>} - Authenticated user profile
   */
  async function login(username, password) {
    const data = await postJson("/api/auth/login", { username, password });
    
    const authenticatedUser = {
      username: data.username,
      role: data.role,
      taxpayerId: data.taxpayerId,
      name: data.role === "Officer" ? `Officer ${data.username}` : data.username,
    };

    // Store JWT token and user profile in sessionStorage
    sessionStorage.setItem("lirs_token", data.token);
    sessionStorage.setItem("lirs_user", JSON.stringify(authenticatedUser));
    setUser(authenticatedUser);

    return authenticatedUser;
  }

  /**
   * Clears the active session and tokens from browser storage and state.
   */
  function logout() {
    sessionStorage.removeItem("lirs_token");
    sessionStorage.removeItem("lirs_user");
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

/**
 * Custom React Hook for easily consuming AuthContext in any component.
 * Usage: const { user, login, logout } = useAuth();
 *
 * @returns {{ user: object|null, login: Function, logout: Function }}
 */
export function useAuth() {
  return useContext(AuthContext);
}
