using System.Security.Cryptography;

namespace ConsoleApp1;

public static class AesCriptografia
{
    public static byte[] EncryptMessage(string message, byte[] key)
    {
        using var aesAlg = new AesManaged();
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
        aesAlg.Key = key;
    
        aesAlg.GenerateIV();
        var iv = aesAlg.IV;

        using var msEncrypt = new MemoryStream();
        using (var encryptor = aesAlg.CreateEncryptor())
        {
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(message);
                }
            }
        }
    
        var encryptedMessage = msEncrypt.ToArray();
        var result = new byte[iv.Length + encryptedMessage.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedMessage, 0, result, iv.Length, encryptedMessage.Length);

        return result;
    }
    public static string DecryptMessage(byte[] encryptedMessage, byte[] key)
    {

        using var aesAlg = new AesManaged();
        aesAlg.Key = key;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;
    
        var iv = new byte[aesAlg.BlockSize / 8];
        Buffer.BlockCopy(encryptedMessage, 0, iv, 0, iv.Length);
    
        var cipherText = new byte[encryptedMessage.Length - iv.Length];
        Buffer.BlockCopy(encryptedMessage, iv.Length, cipherText, 0, cipherText.Length);

        using var msDecrypt = new MemoryStream(cipherText);
        using var decryptor = aesAlg.CreateDecryptor(key, iv);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}