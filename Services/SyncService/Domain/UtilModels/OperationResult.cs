namespace SyncService.Domain.UtilModels
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }

        public T? Data { get; set; }

        public string Message { get; set; } = null!;

        public static OperationResult<T> Success(T? data, string message = "Operation Succesfull")
        {
            return new OperationResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static OperationResult<T> Failure(T? data = default, string message = "Operation failed")
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Data = data,
                Message = message
            };
        }

        public static OperationResult<T> Error(Exception exception, string message = "An error occurred")
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Data = default,
                Message = $"{message}: {exception.Message}"
            };
        }
    }
}
