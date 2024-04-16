# Tubbly.WildDrop.Api.NET

## Usage

```cs
using Tubbly.WildDrop.Api.NET;

internal class Program {
    static async Task Main(string[] args) {
        using var privateKey = WildUtils.ImportRSAFromFile("Key/Private.key", "hardcore salad matter ramp");

        using var api = new WildApi("http://[::1]:9876", privateKey);

        await api.OpenSessionAsync(async (session) => {
            var wallet = await session.CreateWalletAsync();

            Console.WriteLine("New wallet address: {0}", wallet.Address);

            await session.SetWalletBalanceAsync(wallet.Address, 100);
        });
    }
}
```
