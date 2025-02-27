namespace Fitz.Features.Lottery.Jobs.Services
{
    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public ServiceResponse(bool success, string message, object data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public ServiceResponse()
        {
            Success = true;
            Message = string.Empty;
        }
    }
}