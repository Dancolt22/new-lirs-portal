/**
 * ==============================================================================
 * Centralized API Service Client (src/services/api.js) - Day 4
 * ==============================================================================
 * Architecture & Security Principles:
 * 1. Bearer Token Injection: Automatically retrieves the cryptographically signed
 *    JWT from browser sessionStorage and attaches it in the standard
 *    'Authorization: Bearer <token>' header on all outgoing API requests.
 * 2. Single Source of Truth: All network communication to the ASP.NET Core backend
 *    (http://localhost:5123) is routed through this module.
 * 3. Unified Error Handling: Non-2xx responses are parsed and throw descriptive errors
 *    with backend-provided messages (e.g. 403 Forbidden or business rule errors).
 * ==============================================================================
 */

const BASE_URL = "http://localhost:5123";

/**
 * Retrieves the stored JWT token and returns standard Authorization Bearer header.
 *
 * @returns {Record<string, string>} Authorization header object or empty object
 */
function getAuthHeader() {
  const token = sessionStorage.getItem("lirs_token");
  return token ? { Authorization: `Bearer ${token}` } : {};
}

/**
 * Common response handler for fetch promises.
 * Inspects HTTP response status and parses JSON body safely.
 *
 * @param {Response} response - Standard browser Fetch API Response object
 * @returns {Promise<any>} - Parsed JSON body
 * @throws {Error} - Descriptive error extracted from API error payload or fallback
 */
async function handle(response) {
  const data = await response.json().catch(() => ({}));
  if (!response.ok) {
    throw new Error(data.error || "Request failed");
  }
  return data;
}

/**
 * Sends an HTTP GET request to the specified API endpoint with JWT Bearer header.
 *
 * @param {string} path - Relative URL path (e.g. "/api/taxpayers")
 * @returns {Promise<any>} - Response data parsed from JSON
 */
export function getJson(path) {
  return fetch(`${BASE_URL}${path}`, {
    headers: {
      ...getAuthHeader(),
    },
  }).then(handle);
}

/**
 * Sends an HTTP POST request with a JSON payload and JWT Bearer header.
 *
 * @param {string} path - Relative URL path (e.g. "/api/payments")
 * @param {object} body - JavaScript object to be serialized into JSON
 * @returns {Promise<any>} - Response data parsed from JSON
 */
export function postJson(path, body) {
  return fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      ...getAuthHeader(),
    },
    body: JSON.stringify(body),
  }).then(handle);
}
