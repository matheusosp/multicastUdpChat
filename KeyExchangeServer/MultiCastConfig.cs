using System.Security.Cryptography;

namespace KeyExchangeServer;

public class MultiCastConfig
{
    public string IP { get; set; } = "224.0.0.1";
    public int Port { get; set; } = 50000;
    public string SymmetricKey { get; set; } = GenerateSymmetricKey();

    private static string GenerateSymmetricKey()
    {
        var senha = "1234";
        var salt = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 };
        const int iterations = 10000;

        using var kdf = new Rfc2898DeriveBytes(senha, salt, iterations);
        var chave = kdf.GetBytes(32);
        return BitConverter.ToString(chave).Replace("-", "");
    }
}