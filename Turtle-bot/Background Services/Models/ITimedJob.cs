namespace Fitz.BackgroundServices.Models
{
    using System.Threading.Tasks;

    public interface ITimedJob
    {
        int Interval { get; }

        Task Execute();
    }
}
