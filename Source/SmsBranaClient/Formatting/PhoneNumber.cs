namespace SmsBranaClient.Formatting;

public static class PhoneNumber
{
    public static string? NormalizeOrNull(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) 
            return null;

        return phoneNumber
            .Replace(" ", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("-", "");
    }
}