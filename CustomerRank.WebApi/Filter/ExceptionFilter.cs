using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using CustomerRankWebApi.Exception;

namespace CustomerRankWebApi.Filter
{
    //ExceptionFilterAttribute Attribute, IAsyncExceptionFilter
    public class BusinessExceptionFilterAttribute : ExceptionFilterAttribute
    {
       
        private readonly ILogger<BusinessExceptionFilterAttribute> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;
        public BusinessExceptionFilterAttribute(
            ILogger<BusinessExceptionFilterAttribute> logger,
            IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        
        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            _logger.LogError($"{context.HttpContext.Request.Path}, {context.Exception.Message}\n\r {context.Exception.StackTrace}");


            if (context.ExceptionHandled == false)
            {
                var businessException = context.Exception as BusinessException;
                if (businessException != null)
                {
                    var jsonResult = new JsonResult(new
                    {
                        code = businessException.StatusCode,
                        msg = $"Error occurred，{context.Exception.Message}",
                        extdata = "error"
                    });
                    jsonResult.StatusCode = (int)businessException.StatusCode;
                    context.Result = jsonResult;
                    context.ExceptionHandled = true;
                }
                else if (!this._hostEnvironment.IsDevelopment())
                {
                    var jsonResult = new JsonResult(new
                    {
                        code = 500,
                        msg = $"System error occurred",
                        extdata = "error"
                    });
                    jsonResult.StatusCode = 500;
                    context.Result = jsonResult;
                    context.ExceptionHandled = true;

                }
            }
            await Task.CompletedTask;
        }

       
    }
}
