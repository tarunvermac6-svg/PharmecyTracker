using System.Text.Json;
using PharmacyMedicineTracker.Models;

namespace PharmacyMedicineTracker.Services;

public class MedicineStore
{
    private readonly string _filePath;
    private readonly object _sync = new();
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public MedicineStore(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "medicines.json");
    }

    public IReadOnlyList<Medicine> GetMedicines(string? search)
    {
        lock (_sync)
        {
            var medicines = Read();
            if (!string.IsNullOrWhiteSpace(search))
                medicines = medicines.Where(m => m.FullName.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            return medicines.OrderBy(m => m.FullName).ToList();
        }
    }

    public AddMedicineResult AddMedicine(MedicineInput input)
    {
        lock (_sync)
        {
            var medicines = Read();
            var alreadyExists = medicines.Any(m =>
                string.Equals(m.FullName.Trim(), input.FullName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
                return new(AddMedicineStatus.Duplicate, null);

            var medicine = new Medicine { FullName = input.FullName.Trim(), Notes = input.Notes?.Trim() ?? "", ExpiryDate = input.ExpiryDate.Date, Quantity = input.Quantity, Price = input.Price, Brand = input.Brand.Trim() };
            medicines.Add(medicine);
            Save(medicines);
            return new(AddMedicineStatus.Success, medicine);
        }
    }

    public SaleResult RecordSale(Guid id, int quantity)
    {
        lock (_sync)
        {
            var medicines = Read();
            var medicine = medicines.FirstOrDefault(m => m.Id == id);
            if (medicine is null) return new(SaleStatus.NotFound, null);
            if (medicine.Quantity < quantity) return new(SaleStatus.InsufficientStock, null);
            medicine.Quantity -= quantity;
            medicine.Sales.Add(new SaleRecord(DateTime.Now, quantity));
            Save(medicines);
            return new(SaleStatus.Success, medicine);
        }
    }

    public bool DeleteMedicine(Guid id)
    {
        lock (_sync)
        {
            var medicines = Read();
            var medicine = medicines.FirstOrDefault(m => m.Id == id);
            if (medicine is null) return false;

            medicines.Remove(medicine);
            Save(medicines);
            return true;
        }
    }

    private List<Medicine> Read() => JsonSerializer.Deserialize<List<Medicine>>(File.ReadAllText(_filePath), _options) ?? [];
    private void Save(List<Medicine> medicines) => File.WriteAllText(_filePath, JsonSerializer.Serialize(medicines, _options));
}
