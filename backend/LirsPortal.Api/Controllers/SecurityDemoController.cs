using Dapper;
using LirsPortal.Api.Data;
using LirsPortal.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LirsPortal.Api.Controllers;

/// <summary>
/// Educational Demonstration Controller for Security & OWASP Top 10 Defense.
/// 
/// Purpose:
/// Demonstrates side-by-side why SQL injection occurs in vulnerable legacy systems
/// and how parameterized queries in Dapper completely eliminate the vulnerability.
/// 
/// Friday Defense Point (OWASP A03: Injection):
/// "SQL injection occurs when untrusted user input is directly concatenated into SQL statement strings.
/// The database engine cannot tell where the programmer's instructions end and the attacker's data begins.
/// By using parameterized queries with '@ParameterName', the SQL query plan is pre-compiled first.
/// The user input is sent strictly as typed data bytes, making code injection mathematically impossible."
/// </summary>
[ApiController]
[Route("api/demo")]
public class SecurityDemoController(DbConnectionFactory factory) : ControllerBase
{
    /// <summary>
    /// VULNERABLE ENDPOINT: Demonstrates string concatenation vulnerability.
    /// If an attacker inputs: ' OR '1'='1
    /// The resulting SQL becomes:
    /// SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TIN = '' OR '1'='1'
    /// This causes the database to evaluate '1'='1' (true) and dump EVERY taxpayer in Lagos State.
    /// </summary>
    [HttpGet("vulnerable-search")]
    public async Task<IActionResult> VulnerableSearch([FromQuery] string tin)
    {
        // INSECURE: Direct string concatenation of untrusted client input
        string sql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TIN = '" + tin + "'";
        using var db = factory.Create();
        var results = await db.QueryAsync<Taxpayer>(sql);
        return Ok(results);
    }

    /// <summary>
    /// SECURE ENDPOINT: Uses Dapper parameterized SQL queries.
    /// Even if an attacker passes: ' OR '1'='1
    /// The database treats the ENTIRE string as a literal TIN value looking for someone whose TIN
    /// literally equals the string "' OR '1'='1". It returns 0 rows safely.
    /// </summary>
    [HttpGet("secure-search")]
    public async Task<IActionResult> SecureSearch([FromQuery] string tin)
    {
        // SECURE: Parameter placeholder '@Tin' ensures parameter is treated purely as data, never executable code
        const string sql = "SELECT TaxpayerId, TIN, Name, Type, State, Phone FROM Taxpayers WHERE TIN = @Tin";
        using var db = factory.Create();
        var results = await db.QueryAsync<Taxpayer>(sql, new { Tin = tin });
        return Ok(results);
    }
}
