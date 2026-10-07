/**
 * ==============================================================================
 * Portal Header Bar Component (src/components/TopBar.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * Renders the top navigation banner displaying the agency brand ("LIRS Taxpayer Portal"),
 * the current user's name and role, and a logout button that redirects back to /login.
 * ==============================================================================
 */

import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function TopBar() {
  // Retrieve global authentication state and logout action from AuthContext
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  /**
   * Handles user sign-out:
   * 1. Resets the user state to null in AuthContext
   * 2. Navigates the browser to the /login route
   */
  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <header className="topbar">
      <strong>LIRS Taxpayer Portal</strong>
      
      {/* Display user identity and logout button only when authenticated */}
      {user && (
        <span>
          {user.name} ({user.role}){" "}
          <button 
            onClick={handleLogout} 
            style={{ marginLeft: "12px", padding: "6px 12px" }}
            aria-label="Log out of session"
          >
            Log out
          </button>
        </span>
      )}
    </header>
  );
}
