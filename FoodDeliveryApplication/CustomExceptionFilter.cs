using FoodDeliveryApplication.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace FoodDeliveryApplication
{
    /*public class CustomExceptionFilter : System.Web.Mvc.FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filtercontext)
        {
            if (filtercontext.Exception is NotImplementedException)
            {

            }
            else
            {
                
                Log.Error(filtercontext.Exception.ToString());
                filtercontext.Result = new ViewResult()
                {
                    ViewName = "Error"
                };
                filtercontext.ExceptionHandled = true;
            }
        }
    }*/
}
