/**
 * ==============================================================================
 * Application Root & Router Mapping (src/App.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * Top-level application component that configures client-side routing.
 *
 * Structure:
 * 1. AuthProvider: Wraps the application to supply authentication state globally.
 * 2. BrowserRouter: Enables declarative HTML5 pushState routing.
 * 3. Routes:
 *    - /login: Public sign-in page
 *    - /taxpayer: Protected dashboard for taxpayers (role="Taxpayer")
 *    - /officer: Protected directory search for revenue officers (role="Officer")
 *    - *: Catch-all fallback route that redirects invalid URLs back to /login
 * ==============================================================================
 */

import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import LoginPage from "./pages/LoginPage";
import TaxpayerDashboard from "./pages/TaxpayerDashboard";
import OfficerDashboard from "./pages/OfficerDashboard";

export default function App() {
  return (
    // Wrap entire routing tree with AuthProvider so any page can read auth state
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public Sign-In Route */}
          <Route path="/login" element={<LoginPage />} />

          {/* Protected Taxpayer Route: Requires role="Taxpayer" */}
          <Route
            path="/taxpayer"
            element={
              <ProtectedRoute role="Taxpayer">
                <TaxpayerDashboard />
              </ProtectedRoute>
            }
          />

          {/* Protected Officer Route: Requires role="Officer" */}
          <Route
            path="/officer"
            element={
              <ProtectedRoute role="Officer">
                <OfficerDashboard />
              </ProtectedRoute>
            }
          />

          {/* Catch-all redirect: Any unmatched path defaults to /login */}
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
