/**
 * ==============================================================================
 * Revenue Officer Directory & Lookup (src/pages/OfficerDashboard.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * Primary screen for LIRS internal revenue officers.
 *
 * Implemented Features:
 * 1. US-05: Taxpayer Directory & 10-digit TIN Search
 * 2. On-demand balance query per taxpayer to minimize unnecessary server load.
 *
 * Performance Consideration:
 * Rather than fetching balances for every single taxpayer in the database at once,
 * each row provides an interactive "Show" button that fetches the balance live
 * and caches it inside local component state (`balances[id]`).
 * ==============================================================================
 */

import { useState } from "react";
import { getJson } from "../services/api";
import TopBar from "../components/TopBar";
import Naira from "../components/Naira";

export default function OfficerDashboard() {
  // Search and results state
  const [tin, setTin] = useState("");
  const [results, setResults] = useState(null);
  
  // Dictionary of on-demand loaded balances: { [taxpayerId]: numericBalance }
  const [balances, setBalances] = useState({});
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  /**
   * Searches the taxpayer directory:
   * - If a TIN is provided: GET /api/taxpayers?tin={tin}
   * - If the search field is blank: GET /api/taxpayers (lists all records)
   */
  async function handleSearch(event) {
    event.preventDefault();
    setError("");
    setLoading(true);

    try {
      const cleanTin = tin.trim();
      const path = cleanTin 
        ? `/api/taxpayers?tin=${encodeURIComponent(cleanTin)}` 
        : "/api/taxpayers";
        
      const data = await getJson(path);
      setResults(data);
    } catch (err) {
      setError(err.message || "Failed to search taxpayers.");
    } finally {
      setLoading(false);
    }
  }

  /**
   * Fetches the outstanding liability balance on demand for a single taxpayer.
   * Caches the returned number in the balances dictionary.
   *
   * @param {number} id - Taxpayer ID
   */
  async function showBalance(id) {
    try {
      const data = await getJson(`/api/taxpayers/${id}/balance`);
      setBalances((prev) => ({ ...prev, [id]: data.balance }));
    } catch (err) {
      setError(err.message || "Failed to retrieve balance.");
    }
  }

  return (
    <>
      <TopBar />
      <main className="container">
        {/* Search Card */}
        <section className="card">
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
        </section>

        {/* Loading and Feedback States */}
        {loading && <p>Searching state tax records...</p>}
        {error && <p role="alert" className="card error">{error}</p>}
        {results && results.length === 0 && (
          <p className="card">No taxpayers found matching that search.</p>
        )}

        {/* Search Results Table */}
        {results && results.length > 0 && (
          <section className="card table-wrap">
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
                    {/* Support both lowercase and PascalCase property names from backend DTOs */}
                    <td><code>{t.tin || t.TIN}</code></td>
                    <td><strong>{t.name || t.Name}</strong></td>
                    <td>{t.type || t.Type}</td>
                    <td>{t.state || t.State}</td>
                    <td>
                      {/* Check if balance for this taxpayer has already been fetched */}
                      {balances[t.taxpayerId] !== undefined ? (
                        <strong><Naira value={balances[t.taxpayerId]} /></strong>
                      ) : (
                        <button 
                          onClick={() => showBalance(t.taxpayerId)}
                          style={{ padding: "4px 10px", fontSize: "12px" }}
                          aria-label={`Fetch balance for ${t.name || t.Name}`}
                        >
                          Show
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </section>
        )}
      </main>
    </>
  );
}
