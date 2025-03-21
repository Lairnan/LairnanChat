using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace LairnanChat.Plugins.Layer.Implements.Models;

public class AuthUser(string login, string password, string language = "ru-RU")
{
    private const int KeySize = 64;
    private const int Iterations = 350000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    [JsonConstructor]
    public AuthUser() : this("", "")
    {
    }

    public string Language { get; set; } = language;
    public string Login { get; set; } = login;
    public string Password { get; set; } = password;

    [JsonIgnore]
    public string HashedPassword { get; private set; } = "";

    [JsonIgnore]
    public string Entropy { get; private set; } = "";
    
    public void SetPassword(string password)
    {
        var entropy = new byte[KeySize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(entropy);
        Entropy = Convert.ToHexString(entropy);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            entropy,
            Iterations,
            HashAlgorithm,
            KeySize);

        HashedPassword = Convert.ToHexString(hash);
    }

    public bool VerifyPassword(string password)
    {
        var entropy = Convert.FromHexString(Entropy);

        try
        {
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, entropy, Iterations, HashAlgorithm, KeySize);
            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(Password));
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }
}