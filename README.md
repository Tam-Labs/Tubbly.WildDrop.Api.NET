# WildDrop.Api.NET

## Usage

```cs
using WildDrop.Api.NET;

internal class Program {
    static async Task Main(string[] args) {
        using var privateKey = WildUtils.ImportRSAFromFile("Key/Private.key", "hardcore salad matter ramp");

        using var api = new WildApi("http://127.0.0.1:9876", privateKey);

        await api.OpenSessionAsync(async (session) => {
            await session.SetWalletBalanceAsync("d43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d", 100);
        });
    }
}

```
