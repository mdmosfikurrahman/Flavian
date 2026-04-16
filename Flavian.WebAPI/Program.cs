using Microsoft.AspNetCore.Mvc;
using Flavian.Configuration.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureProjectSettings(builder.Configuration);
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

var app = builder.Build();

await app.ApplySqlMigrationsAsync();

app.UseProjectConfiguration();

await app.RunAsync();
