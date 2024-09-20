using System.Net;
using System;
namespace CustomerRankWebApi.Exception
{
    public class BusinessException : System.Exception
    {
        public BusinessException(string message, HttpStatusCode statusCode) 
            : base(message) 
        {
            
            this.StatusCode= statusCode;
        }
        public HttpStatusCode StatusCode { get; set; }
    }
}
