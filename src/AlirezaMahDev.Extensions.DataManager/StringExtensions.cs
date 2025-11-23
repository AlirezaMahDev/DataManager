namespace AlirezaMahDev.Extensions.DataManager;

public static class StringExtensions
{
    extension(string s)
    {
        public String16 AsString16() => s;
        public String32 AsString32() => s;
        public String64 AsString64() => s;
        public String128 AsString128() => s;
        public String256 AsString256() => s;
    }
}