# ABC Pharmacy Medicine Tracker

A small, dependency-free ASP.NET Core 8 application built for the coding assessment.

## What it does

- Shows medicines in a responsive grid (excluding Notes).
- Highlights medicines expiring in under 30 days in red, and stock below 10 in yellow. Expiry takes priority when both apply.
- Searches by medicine name.
- Adds medicines with validation.
- Prevents duplicate medicine names and disables Save while a request is being processed.
- Records sales and immediately reduces available stock.
- Lets the user delete a selected medicine after confirmation.
- Persists medicines and sale records in `Data/medicines.json` on the server.

## Run it

From this folder:

```powershell
dotnet run
```

Open the URL printed by ASP.NET Core (normally `http://localhost:5000` or `https://localhost:5001`).

## API

`GET /api/medicines?search=name` lists/searches inventory.  
`POST /api/medicines` adds a medicine.  
`POST /api/medicines/{id}/sales` records a sale with `{ "quantity": 2 }`.
