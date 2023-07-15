using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    /// <summary>
    /// 自定义异常处理类
    /// </summary>
    public class MyExceptionFilter : IExceptionFilter, IFilterMetadata
    {

        public MyExceptionFilter()
        {

        }

        public void OnException(ExceptionContext context)
        {
            if(!(context.Exception is LogicException))
            {
                BusinessLog businessLog = new BusinessLog();

                businessLog.Error(context?.Exception?.Message, context?.Exception);
            }

            if (context.ExceptionHandled == false)
            {
                string msg = context.Exception.Message;
                context.Result = new ContentResult
                {
                    Content = Newtonsoft.Json.JsonConvert.SerializeObject(new { data = "", msg = msg, result = "failure" }),
                    StatusCode = StatusCodes.Status200OK,
                    ContentType = "text/html;charset=utf-8"
                };
            }
            context.ExceptionHandled = true; //异常已处理了
        }
    }
}
