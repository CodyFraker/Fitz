namespace Fitz.Variables
{
    public static class Users
    {
        public static ulong ProductionBot = 746797148263415989;
        public static ulong DevelopmentBot = 842058955043897394;
        public const ulong DukeofSussex = 244407876683169792;
        public const ulong Spy = 104359875206725632;
        public const ulong Dodecuplet = 103967428408512512;
        public const ulong Fitz = 746797148263415989;
        public const ulong Admin = 118550670994309122;

        public static void MockFakeStub()
        {
            ProductionBot = DevelopmentBot;
        }
    }
}