/**
 * ==============================================================================
 * Authentication Context & State Provider (src/context/AuthContext.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * React Context provides global state across the entire component tree, avoiding
 * "prop drilling" (passing user data down manually through multiple levels).
 *
 * Training Simulation Note:
 * On Day 3, we simulate authentication using pre-configured training users
 * to focus on React UI development, state management, and role-based navigation.
 * Production server authentication (password hashing, ASP.NET Identity, and JWTs)
 * is integrated on Day 4.
 * ==============================================================================
 */

import { createContext, useContext, useState } from "react";

// Create the Context object
const AuthContext = createContext(null);

/**
 * Predefined training user accounts representing different roles and taxpayers:
 * - Adewale (Taxpayer 101 - Business with outstanding balance)
 * - Chioma  (Taxpayer 102 - Individual with ₦0 balance)
 * - Bisi    (LIRS Revenue Officer - Can search all taxpayers)
 */
const USERS = [
  { username: "adewale", password: "pass123", role: "Taxpayer", taxpayerId: 101, name: "Adewale Ventures Ltd" },
  { username: "chioma",  password: "pass123", role: "Taxpayer", taxpayerId: 102, name: "Chioma Okafor" },
  { username: "bisi",    password: "pass123", role: "Officer",  taxpayerId: null, name: "Officer Bisi" },
];

/**
 * AuthProvider wraps the application and exposes user state, login, and logout.
 *
 * @param {object} props
 * @param {React.ReactNode} props.children - Child components that require auth access
 */
export function AuthProvider({ children }) {
  // Currently authenticated user object, or null when logged out
  const [user, setUser] = useState(null);

  /**
   * Validates credentials against training user records.
   * Sets active user state upon success.
   *
   * @param {string} username - Entered username
   * @param {string} password - Entered password
   * @returns {object|null} - User object if valid, null otherwise
   */
  function login(username, password) {
    const found = USERS.find((u) => u.username === username && u.password === password);
    if (!found) return null;
    setUser(found);
    return found;
  }

  /**
   * Clears the active user state, effectively signing the user out.
   */
  function logout() {
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
