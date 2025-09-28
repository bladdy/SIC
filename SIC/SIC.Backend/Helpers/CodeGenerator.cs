namespace SIC.Backend.Helpers;

public class CodeGenerator
{
    private static readonly Random _random = new Random();
    private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateCode(int length = 6)
    {
        char[] buffer = new char[length];

        for (int i = 0; i < length; i++)
        {
            buffer[i] = _chars[_random.Next(_chars.Length)];
        }

        return new string(buffer);
    }
}