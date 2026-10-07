/**
 * ==============================================================================
 * Taxpayer Self-Service Dashboard (src/pages/TaxpayerDashboard.jsx) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * Primary screen for citizen taxpayers.
 *
 * Implemented Features:
 * 1. US-02: Outstanding Liability Card (GET /api/taxpayers/{id}/balance)
 * 2. US-03: Tax Returns History Table (GET /api/taxpayers/{id}/returns)
 * 3. US-04: Live Payment Submission Form (POST /api/payments)
 *
 * Reactive Behavior:
 * When a payment is successfully posted to the API, `setReload(prev => prev + 1)`
 * triggers the `useEffect` hook to fetch updated balances and returns, immediately
 * reflecting the payment without requiring a manual browser refresh.
 * ==============================================================================
 */

import { useState, useEffect } from "react";
import { useAuth } from "../context/AuthContext";
import { getJson, postJson } from "../services/api";
import TopBar from "../components/TopBar";
import Naira from "../components/Naira";

export default function TaxpayerDashboard() {
  const { user } = useAuth();

  // Server data state
  const [balance, setBalance] = useState(null);
  const [returns, setReturns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  
  // Reload counter: incrementing this triggers useEffect to re-query the API
  const [reload, setReload] = useState(0);

  // Payment form state
  const [returnId, setReturnId] = useState("");
  const [amount, setAmount] = useState("");
  const [channel, setChannel] = useState("Bank");
  const [message, setMessage] = useState("");
  const [formError, setFormError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  /**
   * Data loading side-effect:
   * Queries taxpayer balance and tax returns from the ASP.NET Core API.
   * Runs whenever the user's taxpayerId changes or reload counter increments.
   */
  useEffect(() => {
    async function load() {
      try {
        setLoading(true);
        // Concurrent or sequential fetch of balance and return records
        const b = await getJson(`/api/taxpayers/${user.taxpayerId}/balance`);
        const r = await getJson(`/api/taxpayers/${user.taxpayerId}/returns`);
        setBalance(b.balance);
        setReturns(r);
        setError("");
      } catch (err) {
        setError(err.message || "Failed to load taxpayer records.");
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [user.taxpayerId, reload]);

  /**
   * Handles payment submission:
   * 1. Performs client-side validations (selected return, positive amount)
   * 2. Sends HTTP POST request to /api/payments
   * 3. Displays official receipt number upon success
   * 4. Resets form fields and triggers reload counter
   */
  async function handlePay(event) {
    event.preventDefault();
    setMessage("");
    setFormError("");

    // Client-side validation: must select a tax return
    if (!returnId) {
      setFormError("Please choose a return.");
      return;
    }

    // Client-side validation: amount must be positive
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

      // Show success message with backend-generated receipt number
      setMessage(`Payment successful! Official Receipt: ${result.receiptNumber}.`);
      setAmount("");
      
      // Trigger automatic reload of balance and returns from database
      setReload((prev) => prev + 1);
    } catch (err) {
      // Catches server-side business rule exceptions (e.g., overpayment)
      setFormError(err.message || "Payment could not be processed.");
    } finally {
      setSubmitting(false);
    }
  }

  // Filter returns that are ready for payment (exclude draft items)
  const payable = returns.filter((r) => r.status !== "Draft");

  return (
    <>
      <TopBar />
      <main className="container">
        {/* Loading Indicator */}
        {loading && <p>Loading taxpayer records from server...</p>}

        {/* Global Fetch Error Card */}
        {error && <p role="alert" className="card error">{error}</p>}

        {/* Dashboard Modules */}
        {!loading && !error && (
          <>
            {/* Module 1: Outstanding Liability Summary */}
            <section className="card">
              <h2>Outstanding Liability</h2>
              <p className="balance"><Naira value={balance} /></p>
              {balance === 0 && <p className="success">✓ Your account is up to date.</p>}
            </section>

            {/* Module 2: Tax Returns History Table */}
            <section className="card">
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
            </section>

            {/* Module 3: Payment Submission Form */}
            <section className="card">
              <h2>Record a Tax Payment</h2>
              <form onSubmit={handlePay}>
                <label htmlFor="return">Select Tax Return</label>
                <select 
                  id="return" 
                  value={returnId} 
                  onChange={(e) => setReturnId(e.target.value)}
                >
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
                <select 
                  id="channel" 
                  value={channel} 
                  onChange={(e) => setChannel(e.target.value)}
                >
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
            </section>
          </>
        )}
      </main>
    </>
  );
}
