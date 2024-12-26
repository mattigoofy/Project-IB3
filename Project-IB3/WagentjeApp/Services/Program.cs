using System.Security.Cryptography; // Voor cryptografische functies
using System.Text; // Voor tekstcodering

public static class AesEncryption
{
    // 16-byte sleutel voor AES encryptie (128 bits)
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456");
    // 16-byte Initialisatie Vector (IV) voor AES encryptie
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");

    // Methode om een tekst te versleutelen
    public static string Encrypt(string plainText)
    {
        // Controleer of de invoer geldig is
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText"); // Gooi een uitzondering als de tekst null of leeg is
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key"); // Gooi een uitzondering als de sleutel null of leeg is
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV"); // Gooi een uitzondering als de IV null of leeg is

        byte[] encrypted; // Array voor de versleutelde gegevens

        // Maak een nieuwe AES instantie
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key; // Stel de sleutel in
            aesAlg.IV = IV; // Stel de IV in

            // Maak een encryptor aan
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Gebruik een MemoryStream om de versleutelde gegevens op te slaan
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                // Gebruik een CryptoStream om de gegevens te versleutelen
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    // Gebruik een StreamWriter om de platte tekst naar de CryptoStream te schrijven
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText); 
                    }
                }

                // Verkrijg de versleutelde gegevens als byte-array
                encrypted = msEncrypt.ToArray();
            }
        }
        // Converteer de versleutelde byte-array naar een Base64-string en retourneer deze
        return Convert.ToBase64String(encrypted);
    }

    // Methode om een versleutelde tekst te ontsleutelen
    public static string Decrypt(string cipherText)
    {
        // Controleer of de invoer geldig is
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText"); // Gooi een uitzondering als de tekst null of leeg is
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key"); // Gooi een uitzondering als de sleutel null of leeg is
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV"); // Gooi een uitzondering als de IV null of leeg is

        string plaintext = null; // Variabele voor de ontsleutelde tekst

        // Maak een nieuwe AES instantie
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key; // Stel de sleutel in
            aesAlg.IV = IV; // Stel de IV in

            // Maak een decryptor aan
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Gebruik een MemoryStream om de versleutelde gegevens te lezen
            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                // Gebruik een CryptoStream om de gegevens te ontsleutelen
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    // Gebruik een StreamReader om de ontsleutelde gegevens te lezen
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd(); // Lees de ontsleutelde tekst
                    }
                }
            }
        }

        return plaintext; // Retourneer de ontsleutelde tekst
    }
}