using PharmacyMedicineTracker.Models;
using PharmacyMedicineTracker.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<MedicineStore>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/medicines", (MedicineStore store, string? search) =>
    Results.Ok(store.GetMedicines(search)));

app.MapPost("/api/medicines", (MedicineInput input, MedicineStore store) =>
{
    var errors = Validation.ValidateMedicine(input);
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    var result = store.AddMedicine(input);
    if (result.Status == AddMedicineStatus.Duplicate)
        return Results.Conflict(new { message = "A medicine with this name is already in the inventory." });

    return Results.Created("/api/medicines", result.Medicine);
});

app.MapPost("/api/medicines/{id:guid}/sales", (Guid id, SaleInput input, MedicineStore store) =>
{
    if (input.Quantity <= 0)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["quantity"] = ["Sale quantity must be at least 1."] });

    var result = store.RecordSale(id, input.Quantity);
    return result.Status switch
    {
        SaleStatus.NotFound => Results.NotFound(new { message = "Medicine was not found." }),
        SaleStatus.InsufficientStock => Results.BadRequest(new { message = "Not enough quantity in stock." }),
        _ => Results.Ok(result.Medicine)
    };
});

app.MapDelete("/api/medicines/{id:guid}", (Guid id, MedicineStore store) =>
    store.DeleteMedicine(id)
        ? Results.NoContent()
        : Results.NotFound(new { message = "Medicine was not found." }));

app.MapFallbackToFile("index.html");
app.Run();
