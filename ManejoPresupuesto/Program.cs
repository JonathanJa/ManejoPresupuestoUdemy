using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var politicaUsuarioAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
builder.Services.AddControllersWithViews(option =>
{
    option.Filters.Add(new AuthorizeFilter(politicaUsuarioAutenticados));
});
builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();//agregamos si o si para que pueda funcionar el agregado de datos
builder.Services.AddTransient<IServicioUsuario, ServicioUsuario>();
builder.Services.AddTransient<IRepositorioCuenta, RepositorioCuenta>();
builder.Services.AddTransient<IRepositorioCategoria, RepositorioCategoria>();
builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IServicioReportes, ServicioReportes>();
builder.Services.AddTransient<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddTransient < IUserStore < Usuario >, UsuarioStore > ();
builder.Services.AddTransient < SignInManager<Usuario>>();
builder.Services.AddIdentityCore<Usuario>(opciones =>
{
    opciones.Password.RequireDigit = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireUppercase = false;
    opciones.Password.RequireNonAlphanumeric = false;
}).AddErrorDescriber<MensajeDeErrorIdentity>();
builder.Services.AddAuthentication(opcion =>
{
    opcion.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    opcion.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    opcion.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme, opciones=>{
    opciones.LoginPath = "/Usuarios/Login";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transaccion}/{action=Index}/{id?}");

app.Run();
