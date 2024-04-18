namespace WildDrop.Api.NET;

public static class WildExtensions {
    public static string ToHexString(this byte[] bytes) {
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    public static byte[] ToByteArray(this string hexString) {
        return Convert.FromHexString(hexString);
    }
}

