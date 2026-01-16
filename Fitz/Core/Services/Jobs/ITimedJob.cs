namespace Fitz.Core.Services.Jobs
{
    using System.Threading.Tasks;

    public interface ITimedJob
    {
        ulong Emoji { get; }

        string Interval { get; }

        Task Execute();
    }
}
