namespace Fitz.Variables
{
    public static class Guilds
    {
        public static ulong DodeDuke { get; private set; } = 196820438398140417;

        public static ulong Waterbear { get; private set; } = 1022879771526451240;

        public static ulong RSS { get; private set; } = 864940326363201576;

        public static ulong Icons { get; private set; } = 927594030618013746;

        /// <summary>
        /// Convert guilds to use a specific guild here when debugging.
        /// Will be ignored when running in production.
        /// </summary>
        public static void MockFakeStub()
        {
            Waterbear = DodeDuke;
        }
    }
}