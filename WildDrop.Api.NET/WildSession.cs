namespace WildDrop.Api.NET;

public class WildSession {
    class QueryBody {
        public string QueryName { get; set; } = string.Empty;
        public object[] Arguments { get; set; }
    }

    class TransactBody {
        public string TransactName { get; set; } = string.Empty;
        public object[] Arguments { get; set; }
    }

    class ResultBody {
        public object Result { get; set; }
    }

    public string Hash => SessionHashHex;

    readonly HttpClient HttpClient;
    readonly string Url;
    readonly RSA Key;
    readonly string SessionHashHex;

    readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions() {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    internal WildSession(HttpClient client, string url, RSA key, string sessionHashHex) {
        HttpClient = client;
        Url = url;
        Key = key;
        SessionHashHex = sessionHashHex;
    }

    public async Task SetWalletBalanceAsync(string address, long balance) {
        var request = new HttpRequestMessage() {
            RequestUri = new Uri($"{Url}/wallet/{address}"),
            Method = HttpMethod.Put,
            Content = new StringContent(JsonSerializer.Serialize(new {
                Balance = balance
            }, JsonOptions), new MediaTypeHeaderValue("application/json"))
        };

        request.Headers.Add("wd-session", SessionHashHex);

        var response = await HttpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task<GetWalletBalanceResponse> GetWalletBalanceAsync(string address) {
        var request = new HttpRequestMessage() {
            RequestUri = new Uri($"{Url}/wallet/{address}"),
            Method = HttpMethod.Get
        };

        request.Headers.Add("wd-session", SessionHashHex);

        var response = await HttpClient.SendAsync(request);
        if (response == null) {
            throw new Exception("No response");
        }

        return await DecryptResponseAsync<GetWalletBalanceResponse>(response);
    }

    public async Task<GetWalletsResponse> GetWalletsAsync() {
        var request = new HttpRequestMessage() {
            RequestUri = new Uri($"{Url}/wallet"),
            Method = HttpMethod.Get
        };

        request.Headers.Add("wd-session", SessionHashHex);

        var response = await HttpClient.SendAsync(request);
        if (response == null) {
            throw new Exception("No response");
        }

        return await DecryptResponseAsync<GetWalletsResponse>(response);
    }

    public async Task<CreateWalletResponse> CreateWalletAsync() {
        var request = new HttpRequestMessage() {
            RequestUri = new Uri($"{Url}/wallet"),
            Method = HttpMethod.Post
        };

        request.Headers.Add("wd-session", SessionHashHex);

        var response = await HttpClient.SendAsync(request);
        if (response == null) {
            throw new Exception("No response");
        }

        return await DecryptResponseAsync<CreateWalletResponse>(response);
    }

    async Task<T> DecryptResponseAsync<T>(HttpResponseMessage response) where T : class {
        var encryptedKey = response.Headers.GetValues("wd-enc-key").FirstOrDefault();
        var encryptedIv = response.Headers.GetValues("wd-enc-iv").FirstOrDefault();

        if (string.IsNullOrWhiteSpace(encryptedKey) || string.IsNullOrWhiteSpace(encryptedIv)) {
            throw new Exception("Missing headers");
        }

        var (decryptedKey, decryptedIv) = await Task.Factory.StartNew(() => {
            var decryptedKey = Key.Decrypt(encryptedKey.ToByteArray(), RSAEncryptionPadding.Pkcs1);
            var decryptedIv = Key.Decrypt(encryptedIv.ToByteArray(), RSAEncryptionPadding.Pkcs1);

            return (decryptedKey, decryptedIv);
        });

        var content = await response.Content.ReadAsByteArrayAsync();

        using var aes = Aes.Create();

        aes.Key = decryptedKey;
        aes.IV = decryptedIv;
        aes.Mode = CipherMode.CBC;

        using var decipher = aes.CreateDecryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream(content);
        using var cs = new CryptoStream(ms, decipher, CryptoStreamMode.Read);
        using var mso = new MemoryStream();

        await cs.CopyToAsync(mso);

        var data = Encoding.UTF8.GetString(mso.ToArray());

        return JsonSerializer.Deserialize<T>(data, JsonOptions);
    }
}
