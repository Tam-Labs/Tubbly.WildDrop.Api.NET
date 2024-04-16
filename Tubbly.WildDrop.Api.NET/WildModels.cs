namespace Tubbly.WildDrop.Api.NET;

public record CreateWalletResponse(string Address);
public record GetWalletsResponse(List<string> Addresses);
public record GetWalletBalanceResponse(long Balance);
