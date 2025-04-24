using BlazorApp2.Data;
using BlazorApp2.Data.Services.Auth;
using BlazorApp2.Data.Services.Books;
using BlazorApp2.Data.Services.Fines;
using BlazorApp2.Data.Services.Mails;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<LibDbContext>();
builder.Services.AddTransient<MailService>();
builder.Services.AddSingleton<VerificationService>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider=>
    provider.GetRequiredService<UserSession>());
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<BorrowService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<FineService>();


builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "LibraryAuhtCookie";
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.AccessDeniedPath="/access-denied";
    });
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly",policy=>policy.RequireRole("Admin"));
    options.AddPolicy("LibrarianOnly", policy => policy.RequireRole("Librarian"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

//builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) => {
    if (!context.Request.IsHttps)
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("仅支持HTTPS连接");
        return;
    }
    await next();
});
app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
