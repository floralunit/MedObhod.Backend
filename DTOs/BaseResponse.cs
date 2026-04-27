namespace MedObhod.Backend.DTOs;

public class BaseResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static BaseResponse<T> Ok(T data, string message = "Success")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static BaseResponse<T> Created(T data, string message = "Created successfully")
    {
        return new BaseResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 201
        };
    }

    public static BaseResponse<T> Error(string message, int statusCode = 400)
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static BaseResponse<T> NotFound(string message = "Resource not found")
    {
        return new BaseResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 404
        };
    }
}