using Asp.Versioning.ApiExplorer;
using PRN232.Lab1.CoffeeStore.API;
using PRN232.Lab1.CoffeeStore.API.Utils;
using PRN232.Lab1.CoffeeStore.Repositories;
using PRN232.Lab1.CoffeeStore.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP requestV2 pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{desc.GroupName}/swagger.json",
                $"CoffeeStore API {desc.GroupName.ToUpperInvariant()}" + (desc.IsDeprecated ? " (deprecated)" : "")
            );
        }
    });

    await app.InitialiseDatabaseAsync();
}

app.UseExceptionHandler();
app.UseRouting();
app.UseCors("AllowLocationHeader");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html", false);
    return Task.CompletedTask;
});
app.MapControllers();

app.Run();
