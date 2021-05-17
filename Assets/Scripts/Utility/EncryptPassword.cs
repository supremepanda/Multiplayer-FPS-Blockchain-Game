public static class EncryptPassword 
{
    public static string Encrypt(string password)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] encryptedPassword = System.Text.Encoding.UTF8.GetBytes(password);
        encryptedPassword = provider.ComputeHash(encryptedPassword);
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        foreach(byte b in encryptedPassword)
        {
            str.Append(b.ToString("x2").ToLower());
        }

        return str.ToString();
    }
}
