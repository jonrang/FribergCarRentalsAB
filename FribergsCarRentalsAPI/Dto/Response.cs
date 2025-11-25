namespace FribergCarRentalsAPI.Dto
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public T? Data { get; set; }
    }
}
