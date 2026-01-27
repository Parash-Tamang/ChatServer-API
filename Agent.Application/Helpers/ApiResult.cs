using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Helpers
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public static ApiResult<T> Ok(T? data, string message = "Success")
             => new ApiResult<T>
             {
                 Success = true,
                 Data = data,
                 Message = message
             };

        public static ApiResult<T> Fail(string message)
            => new ApiResult<T>
            {
                Success = false,
                Message = message,
                Data = default
            };
    }
}
