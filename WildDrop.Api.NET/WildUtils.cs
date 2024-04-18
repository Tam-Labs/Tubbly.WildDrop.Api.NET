namespace WildDrop.Api.NET;

public static class WildUtils {
    public static RSA ImportRSAFromFile(string path, string passphrase) {
        if (path == null) {
            throw new ArgumentNullException(nameof(path));
        }

        if (passphrase == null) {
            throw new ArgumentNullException(nameof(passphrase));
        }

        var privateKey = RSA.Create();

        privateKey.ImportFromEncryptedPem(File.ReadAllText(path), Encoding.UTF8.GetBytes(passphrase));

        return privateKey;
    }
}

