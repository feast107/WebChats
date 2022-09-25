using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MonitorHub.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using M = MonitorHub.Hubs.MonitorHub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TokenOptions>(builder.Configuration.GetSection("Authentication"));
var token = builder.Configuration.GetSection("Authentication").Get<TokenOptions>();

// Add services to the container.
builder.Services.AddRazorPages();
//添加实时应用
builder.Services.AddSignalR(o=> { 
    o.MaximumReceiveMessageSize = 2097152; 
});
//添加验证
builder.Services.AddAuthorization();
//添加授权
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)),
        ValidIssuer = token.Issuer,

        ValidAudience = token.Audience,
        ValidateIssuer = false,
        ValidateAudience = false
    };

    //添加验证失败事件
    o.Events = new JwtBearerEvents()
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.HttpContext.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!(string.IsNullOrWhiteSpace(accessToken))
                && path.StartsWithSegments("/Monitor"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SignalRServer", Version = "v1" });
    c.DocInclusionPredicate((docName, description) => true);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT授权(数据将在请求头中进行传输) 在下方输入Bearer {token} 即可，注意两者之间有空格",
        Name = "Authorization",//jwt默认的参数名称
        In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
        Type = SecuritySchemeType.ApiKey
    });
    //认证方式，此方式为全局添加
    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement 
        {
            { 
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference()
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                }, Array.Empty<string>() 
            }
        });
});
builder.Services.AddDistributedMemoryCache();//分布式缓存
builder.Services.AddControllersWithViews();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//启用Swagger
app.UseSwagger();
//启用Swagger视图
app.UseSwaggerUI(options =>
{
    options.ShowExtensions();
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI V1");
    options.DocExpansion(DocExpansion.None);
    options.DocumentTitle = "MonitorHub";
    options.HeadContent = "<link rel=\"icon\" type=\"image/png\" href=\"/rowss.png\" />\n" +
    "<link rel=\"shortcut icon\" type=\"image/png\" href=\"/rowss.png\" />\n";
});
//启用Http重定向
app.UseHttpsRedirection();
//启用静态文件
app.UseStaticFiles();
//启用授权
app.UseAuthentication();
//启用路由
app.UseRouting();

//启用验证
app.UseAuthorization();
//启用终结点
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Document}/{action=Swagger}/{id?}");
});
//启用Razor视图
app.MapRazorPages();
//启用中心
app.MapHub<M>("/Monitor", (o) => {

});
//启动
app.Run();