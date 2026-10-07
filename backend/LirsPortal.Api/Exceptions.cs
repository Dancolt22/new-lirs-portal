namespace LirsPortal.Api;

public class NotFoundException(string message) : Exception(message);
public class BusinessRuleException(string message) : Exception(message);
