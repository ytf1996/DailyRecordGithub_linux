using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyNetCore.Business;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using Senparc.Weixin.RegisterServices;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.CO2NET;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP.Containers;
using MyNetCore.Models;
using Quartz;
using Quartz.Impl;
using MyNetCore.Business.Signal;
using Senparc.Weixin.Work;
using MyNetCore.Business.Jobs;

namespace MyNetCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc(
                opt =>
                {
                    //自定义异常处理
                    opt.Filters.Add<MyExceptionFilter>();
                }
                ).SetCompatibilityVersion(CompatibilityVersion.Latest);

            //配置SignalR服务 配置跨域
            services.AddCors(options => options.AddPolicy("CorsPolicy",
                                                          builder =>
                                                          {
                                                              builder.AllowAnyMethod()
                                                                  .AllowAnyHeader()
                                                                  .SetIsOriginAllowed(str => true)
                                                                  .AllowCredentials();
                                                          }));
            services.AddSignalR();
            services.AddControllers();

            services.AddRouting(options => options.LowercaseUrls = true);

            //添加认证Cookie信息
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
             .AddCookie(options =>
             {
                 options.LoginPath = new PathString("/Account/Login");
                 //options.AccessDeniedPath = new PathString("/denied");
             });

            services.AddHttpContextAccessor();

            services.AddSession();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;//这里要改为false，默认是true，true的时候session无效
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                    .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册

            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting
            , ISchedulerFactory schedulerFactory)
        {
            BusinessHelper.DBConfig = Configuration.GetConnectionString("MySqlConnection");

            BusinessHelper.TlbbDBConfig = Configuration.GetConnectionString("MySqlTLBBConnection");

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            string pathRoot = Directory.GetCurrentDirectory();

            app.UseSession();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            AppContextMy.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            app.UseRouting();

            //使用跨域			
            app.UseCors("CorsPolicy");

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "areaRoute",
                    pattern: "{area:exists}/{controller}/{action}/{id?}");

                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                routes.MapControllers();
                //使用集线器
                routes.MapHub<SignalRHub>("/chatHub");
            });

            app.UseWebSockets();

            // 启动 CO2NET 全局注册，必须！
            IRegisterService register = RegisterService.Start(senparcSetting.Value).UseSenparcGlobal(false, null);

            //开始注册微信信息，必须！
            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value);

            AccessTokenContainer.RegisterAsync(WeChatMiniSettingParam.MyConfig.WxOpenAppId, WeChatMiniSettingParam.MyConfig.WxOpenAppSecret);

            BusinessHelper.InitData();
            JobBaseService.RunTask(schedulerFactory, false);
        }

    }
}
