using CustomerProfileAPI.Models;
using Steeltoe.Extensions.Configuration.Kubernetes.ServiceBinding;
using Steeltoe.Management.Endpoint;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKubernetesServiceBindings();
builder.Services.AddDbContext<CustomerProfilesContext>();
builder.Services.AddSingleton<ICertStoreContext, LocalCertStoreContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Learn more about management endpoints at https://docs.steeltoe.io/api/v3/management/
builder.AddAllActuators();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();