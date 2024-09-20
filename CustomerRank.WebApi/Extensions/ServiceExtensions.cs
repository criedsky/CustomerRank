using CustomerRankWebApi.Filter;
using CustomerRankWebApi.Service;
using System.Runtime.CompilerServices;

namespace CustomerRankWebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddDomainService(this IServiceCollection services)
        {
            services.AddScoped<ICustomerService, CustomerService>();
        }

        public static void AddBusinessExceptionFilter(this IServiceCollection services) 
        {
            services.AddControllers(MvcOptions =>
            {
                // 全局注册Filter：对当前项目下所有的Action都生效；可以支持IOC容器的创建；    
                MvcOptions.Filters.Add<BusinessExceptionFilterAttribute>();
            });
        }
    }
}
