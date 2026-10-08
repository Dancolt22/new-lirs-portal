namespace LirsPortal.Api;

/// <summary>
/// Thrown when a requested domain entity (such as a Taxpayer or Return) cannot be located in the database.
/// Handled by controllers or middleware to return an HTTP 404 Not Found response.
/// </summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>
/// Thrown when a client action violates an established business rule or tax policy
/// (such as attempting an overpayment, or paying against an unapproved draft return).
/// Handled by controllers or middleware to return an HTTP 400 Bad Request response.
/// </summary>
public class BusinessRuleException(string message) : Exception(message);
