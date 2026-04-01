using RagDemo.Api.HttpContexts;
using RagDemo.Api.HostedServices;
using RagDemo.Api.Managers;
using RagDemo.Api.Services;
using RagDemo.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// RFC 7807-compliant error reporting
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<UserSessionManager>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<IUserSessionHttpContext, UserSessionHttpContext>();

builder.Services.AddHostedService<UserSessionExpiryService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(option =>
{
   option.AddDefaultPolicy(p =>
        p.SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseCors();
app.MapControllers();

app.Run();

