namespace PharmacyMedicineTracker.Models;

public class Medicine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public List<SaleRecord> Sales { get; set; } = [];
}

public record MedicineInput(string FullName, string? Notes, DateTime ExpiryDate, int Quantity, decimal Price, string Brand);
public record SaleInput(int Quantity);
public record SaleRecord(DateTime SoldAt, int Quantity);

public enum SaleStatus { Success, NotFound, InsufficientStock }
public record SaleResult(SaleStatus Status, Medicine? Medicine);

public enum AddMedicineStatus { Success, Duplicate }
public record AddMedicineResult(AddMedicineStatus Status, Medicine? Medicine);

public static class Validation
{
    public static Dictionary<string, string[]> ValidateMedicine(MedicineInput input)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(input.FullName)) errors["fullName"] = ["Medicine name is required."];
        if (string.IsNullOrWhiteSpace(input.Brand)) errors["brand"] = ["Brand is required."];
        if (input.ExpiryDate == default) errors["expiryDate"] = ["Expiry date is required."];
        if (input.Quantity < 0) errors["quantity"] = ["Quantity cannot be negative."];
        if (input.Price < 0) errors["price"] = ["Price cannot be negative."];
        return errors;
    }
}
