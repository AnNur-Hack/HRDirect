namespace HRDirectAPI.Helpers;


public static class PasswordHasher
{
    public static string EncryptPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new Exception("Password cannot be empty");
        }
        
        var encryptPass = BCrypt.Net.BCrypt.HashPassword(password);
        return encryptPass;
    }

    public static bool VerifyHashedPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new Exception("Password cannot be empty");
        }
        
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}