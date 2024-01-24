using FluentValidation;
using LearnEnglish.Authentication;
using Masa.BuildingBlocks.Dispatcher.Events;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region 配置Serilog日志
builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration.ReadFrom
            .Configuration(context.Configuration)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());
#endregion
// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMasaDbContext<LearnEnglishAuthenitcationContext>(optionsBuilder =>
{
    optionsBuilder.UseSqlServer();

    optionsBuilder.EnableSoftDelete = true;//启用软删除

    optionsBuilder.UseFilter();//启用软删除过滤

    optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);//默认不启用跟踪查询

    optionsBuilder.Builder = (serverProvider, builder) =>
    {
        builder.UseOpenIddict<Guid>();//配置Openiddict 默认实体
    };
});

#region 事件总线

builder.Services.AddEventBus();//进程内事件总线

builder.Services.AddValidatorsFromAssembly(Assembly.GetEntryAssembly());//批量注册验证模型
builder.Services.AddEventBus(eventBusBuilder => eventBusBuilder.UseMiddleware(typeof(ValidatorEventMiddleware<>)));//模型验证
#endregion

#region 配置OpenIddict
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<LearnEnglishAuthenitcationContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("connect/token");


        options.UseReferenceAccessTokens()
               .UseReferenceRefreshTokens();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();

        options.UseAspNetCore();
    });
#endregion

#region 配置友好异常日志记录
builder.Services.Configure<MasaExceptionLogRelationOptions>(options =>
{
    options.MapLogLevel<UserFriendlyException>(LogLevel.Warning);
});
#endregion

var app = builder.AddServices();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region 配置友好异常日志记录
app.UseMasaExceptionHandler(options =>
{
    options.ExceptionHandler = async context =>
    {
        if(context.Exception is UserFriendlyException ex)
        {
            context.StatusCode = 500;

            var response = new
            {
                Message = ex.Message,
                Code = 141
            };

            await context.HttpContext.Response.WriteAsJsonAsync(response);
        }
    };
});
#endregion

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

await app.RunAsync();
