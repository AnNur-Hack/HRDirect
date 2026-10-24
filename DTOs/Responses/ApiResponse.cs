namespace HRDirectAPI.DTOs.Responses;

public class ApiResponse
{
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public object? Data { get; set; }

    public static class ResponseHelper
    {
        public static ApiResponse SuccessResponse(
            object? data,
            string message = "Operation completed successfully")
        {
            return new ApiResponse
            {
                ResponseCode = "00",
                ResponseMessage = message,
                Data = data
            };
        }

        public static ApiResponse FailureResponse(string message)
        {
            return new ApiResponse
            {
                ResponseCode = "99",
                ResponseMessage = message,
                Data = null
            };
        }
    }
}