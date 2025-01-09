namespace Palig.ICSS.support.Services.Interfaces
{
    public interface IAESHelper
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
