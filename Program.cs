using RipgrepUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

builder.Services.AddScoped<RipgrepService>();
builder.Services.AddScoped<DirectoryBrowserService>();
builder.Services.AddSingleton<EditorSettingsService>();
builder.Services.AddScoped<EditorLaunchService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Only use HTTPS redirect in production web hosting, not for standalone executable
if (app.Environment.IsProduction() && !args.Contains("--standalone"))
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();