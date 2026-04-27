var builder = WebApplication.CreateBuilder(args);

// Dodajemy obsługę kontrolerów
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mapujemy kontrolery
app.MapControllers();

app.Run();