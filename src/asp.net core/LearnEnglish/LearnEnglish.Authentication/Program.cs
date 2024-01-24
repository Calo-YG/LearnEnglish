using FluentValidation;
using LearnEnglish.Authentication;
using Masa.BuildingBlocks.Dispatcher.Events;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

#region ����Serilog��־
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

    optionsBuilder.EnableSoftDelete = true;//������ɾ��

    optionsBuilder.UseFilter();//������ɾ������

    optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);//Ĭ�ϲ����ø��ٲ�ѯ

    optionsBuilder.Builder = (serverProvider, builder) =>
    {
        builder.UseOpenIddict<Guid>();//����Openiddict Ĭ��ʵ��
    };
});

#region �¼�����

builder.Services.AddEventBus();//�������¼�����

builder.Services.AddValidatorsFromAssembly(Assembly.GetEntryAssembly());//����ע����֤ģ��
builder.Services.AddEventBus(eventBusBuilder => eventBusBuilder.UseMiddleware(typeof(ValidatorEventMiddleware<>)));//ģ����֤
#endregion

#region ����OpenIddict
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

#region �����Ѻ��쳣��־��¼
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

#region �����Ѻ��쳣��־��¼
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
