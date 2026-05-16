namespace ProjectHubAPI.Common.Responses
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }

        public static ServiceResponse<T> Ok(T data, string message = "") 
            => new() { Data = data, Success = true, Message = message };

        public static ServiceResponse<T> Fail(string message, List<string>? errors = null) 
            => new() { Success = false, Message = message, Errors = errors };
    }
}

