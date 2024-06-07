namespace Fitz.Variables
{
    public static class Roles
    {
        // Replace with a real role ID.
        public static ulong exampleRole { get; set; } = 103940914665242624;

        public static ulong Accounts { get; private set; } = 1233236506492014602;

        public static ulong MockAccounts { get; private set; } = 889955578313605130;

        public static void MockFakeStub()
        {
            Accounts = MockAccounts;
        }
    }
}