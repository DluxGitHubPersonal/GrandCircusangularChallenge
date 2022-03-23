var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddTransient(typeof(GrandCircusAspWithAngular.Controllers.OpenWeatherClient));
builder.Logging.ClearProviders();
builder.Logging.AddDebug();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Install NET CORE side logging to debug window for exceptions generated while processing requests
app.Logger.LogInformation("Application starting");
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var requestProcessingErrorInfo = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        if (requestProcessingErrorInfo != null)
        {
            var error = requestProcessingErrorInfo?.Error;
            if (error != null)
            {
                app.Logger.LogError($"Endpoint {requestProcessingErrorInfo?.Path} threw error {error?.GetType().Name}:  {error?.Message}");
                app.Logger.LogError($"{error?.StackTrace}");
            }
            else
            {
                // This should never happen
                app.Logger.LogError($"Endpoint {requestProcessingErrorInfo?.Path} threw undescribed error!");
            }
        }
        else
        {
            // This should never happen
            app.Logger.LogError($"Framework error occured withOUT IExceptionHandlerPathFeature description!");
        }

    });
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapFallbackToFile("index.html"); ;

app.Run();
app.Logger.LogInformation("Application exiting\n");