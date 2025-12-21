using ShiftTrack.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// API client (HttpClientFactory)
builder.Services.AddHttpClient<ShiftTrackApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Api:BaseUrl is not configured.");

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ShiftDefinitions}/{action=Index}/{id?}");

app.Run();
