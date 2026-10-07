/**
 * ==============================================================================
 * Centralized API Service Client (src/services/api.js) - Day 3
 * ==============================================================================
 * Architecture Purpose:
 * Instead of having raw `fetch()` calls scattered across different React components,
 * all network communication is centralized here.
 *
 * Benefits:
 * 1. Single source of truth for the Backend API base URL and port.
 * 2. Unified error handling: catches non-2xx HTTP responses (400, 404, 500)
 *    and extracts server error messages so components receive standard Error objects.
 * 3. Consistent request headers (JSON Content-Type, encoding).
 * ==============================================================================
 */

// Port 5123 matches the ASP.NET Core API launched with `dotnet run --urls http://localhost:5123`
const BASE_URL = "http://localhost:5123";

/**
 * Common response handler for fetch promises.
 * Inspects HTTP response status and parses JSON body safely.
 *
 * @param {Response} response - Standard browser Fetch API Response object
 * @returns {Promise<any>} - Parsed JSON body
 * @throws {Error} - Descriptive error extracted from API error payload or fallback
 */
async function handle(response) {
  // Gracefully handle empty or non-JSON payloads
  const data = await response.json().catch(() => ({}));

  // If HTTP status is not 200-299, throw with backend error message
  if (!response.ok) {
    throw new Error(data.error || "Request failed");
  }

  return data;
}

/**
 * Sends an HTTP GET request to the specified API endpoint.
 *
 * @param {string} path - Relative URL path (e.g. "/api/taxpayers")
 * @returns {Promise<any>} - Response data parsed from JSON
 */
export function getJson(path) {
  return fetch(`${BASE_URL}${path}`).then(handle);
}

/**
 * Sends an HTTP POST request with a JSON payload to the specified API endpoint.
 *
 * @param {string} path - Relative URL path (e.g. "/api/payments")
 * @param {object} body - JavaScript object to be serialized into JSON
 * @returns {Promise<any>} - Response data parsed from JSON
 */
export function postJson(path, body) {
  return fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: { 
      "Content-Type": "application/json" 
    },
    body: JSON.stringify(body),
  }).then(handle);
}
