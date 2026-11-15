using System.Security.Cryptography;
using System.Text;

namespace LibraryApp.Services;

// SHA-256 в нижнем регистре - как в скрипте базы данных.
public static class PasswordHelper
{
    public static string Hash(string password) =>
        Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(password)))
        .ToLowerInvariant();
}
