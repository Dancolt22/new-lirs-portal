/**
 * ==============================================================================
 * Route Guard Component (src/components/ProtectedRoute.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * Acts as an access boundary for routes in React Router.
 *
 * Capabilities:
 * 1. Unauthenticated check: Redirects anonymous visitors to `/login`.
 * 2. Role-based authorization: If a specific `role` is required (e.g. "Officer")
 *    and the active user lacks that role (e.g. "Taxpayer"), renders an Access Denied card.
 *
 * Security Note:
 * Client-side route guarding provides a clean user experience (preventing navigation
 * to screens the user cannot use). True security must always be enforced on the
 * server API with authentication and authorization checks (covered on Day 4).
 * ==============================================================================
 */

import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

/**
 * ProtectedRoute
 *
 * @param {object} props
 * @param {string} [props.role] - Optional required role ("Taxpayer" | "Officer")
 * @param {React.ReactNode} props.children - Target component to render if access is permitted
 */
export default function ProtectedRoute({ role, children }) {
  const { user } = useAuth();
  
  // If user is not logged in, redirect immediately to login screen
  if (!user) {
    return <Navigate to="/login" replace />;
  }
  
  // If user does not possess the required role, show an explicit permission error
  if (role && user.role !== role) {
    return (
      <div className="container" style={{ marginTop: "40px" }}>
        <p className="card error" role="alert">
          You are not allowed to view this page. Required role: {role}.
        </p>
      </div>
    );
  }
  
  // Access granted: render protected child components
  return children;
}
