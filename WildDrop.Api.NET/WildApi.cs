namespace WildDrop.Api.NET;

public class WildApi : IDisposable {
    readonly RSA Key;
    readonly byte[] PublicKeyDer;
    readonly string PublicKeyBase64;
    readonly string Url;
    readonly HttpClient HttpClient = new HttpClient();

    public WildApi(string url, RSA privateKey) {
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Key = privateKey ?? throw new ArgumentNullException(nameof(privateKey));

        PublicKeyDer = Key.ExportRSAPublicKey();
        PublicKeyBase64 = Convert.ToBase64String(PublicKeyDer);
    }

    public async Task OpenSessionAsync(Func<WildSession, Task> callback) {
        if (callback == null) {
            throw new ArgumentNullException(nameof(callback));
        }

        var sessionHashHex = await OpenAsync();
        if (string.IsNullOrWhiteSpace(sessionHashHex)) {
            throw new Exception("Cannot establish connection.");
        }

        try {
            await callback.Invoke(new WildSession(HttpClient, Url, Key, sessionHashHex));
        } catch (Exception) {
            throw;
        } finally {
            await CloseAsync(sessionHashHex);
        }
    }

    async Task<string> OpenAsync() {
        var response = await HttpClient.PostAsync($"{Url}/open", new StringContent(PublicKeyBase64));

        var encryptedSessionHash = await response.Content.ReadAsByteArrayAsync();

        var decryptedSessionHash = Key.Decrypt(encryptedSessionHash, RSAEncryptionPadding.Pkcs1);

        return decryptedSessionHash.ToHexString();
    }

    async Task CloseAsync(string sessionHashHex) {
        var request = new HttpRequestMessage() {
            RequestUri = new Uri($"{Url}/close"),
            Method = HttpMethod.Post
        };

        request.Headers.Add("wd-session", sessionHashHex);

        await HttpClient.SendAsync(request);
    }

    public void Dispose() {
        HttpClient.Dispose();
    }
}
