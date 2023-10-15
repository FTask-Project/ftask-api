using System.Security.Cryptography;

namespace FTask.API.Service;

public interface IKeyManager
{
    public RSA RsaKey { get; }
}
public class KeyManager : IKeyManager
{
    public RSA RsaKey { get; }

    public KeyManager()
    {
        RsaKey = RSA.Create();
        if (File.Exists("key"))
        {
            RsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
        }
        else
        {
            var privateKey = RsaKey.ExportRSAPrivateKey();
            File.WriteAllBytes("key", privateKey);
        }
    }
}
