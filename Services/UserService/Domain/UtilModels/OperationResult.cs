namespace UserService.Domain.UtilModels
{
    public record OperationResult<T>
    {
        public string? Message { get; init; }

        public T? Data { get; init; } = default;

        public bool IsSuccess { get; init; }

        public static OperationResult<T> Success(string message = "Operation successful", T? data = default)
        {
            return new OperationResult<T>
            {
                Message = message,
                Data = data,
                IsSuccess = true
            };
        }

        public static OperationResult<T> Failure(string message = "Operation failed",T? data = default)
        {
            return new OperationResult<T>
            {
                Message = message,
                Data = data,
                IsSuccess = false
            };
        }

    
    }
}
