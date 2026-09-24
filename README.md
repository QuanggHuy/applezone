# AppleZone — Apple Store E-commerce

An online store for Apple products (iPhone, Mac, iPad, Apple Watch, accessories), built with **ASP.NET Core 8 MVC + EF Core + SQL Server**, focusing on transaction safety, role-based authorization, and secure data access.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8 MVC, C# |
| Database | SQL Server 2022 |
| ORM | Entity Framework Core |
| Auth | ASP.NET Core Identity |
| Frontend | Razor Views, Bootstrap |
| Infrastructure | Docker Compose |

## Key Features

- Product catalog: search, filter (category / price / discount), sort, detail page
- Checkout with pessimistic locking (prevents overselling) and a single SQL Transaction (all-or-nothing stock deduction)
- Role-based authorization (Admin / Employee / Customer) via ASP.NET Core Identity
- IDOR protection — customers can only view their own orders
- SQL Injection prevention via parameterized queries (EF Core)
- Order status state machine (pending → shipping → delivered / cancelled), auto stock restore on cancel
- Admin dashboard: revenue, order status breakdown, top products, low-stock alerts, filter by month/year

## Project Structure

```text
applezone/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── Helper/
├── sql/                  # schema scripts (no EF Migrations)
└── docker-compose.yml
```

## Getting Started

```bash
cp .env.example .env
# edit .env, set SA_PASSWORD

docker-compose up -d
# wait a few seconds for SQL Server to accept connections

dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=applezone;User Id=sa;Password=<SA_PASSWORD>;TrustServerCertificate=True"

# run each script in sql/ once, in filename order (Database-First, no EF Migrations)
docker exec -i applezone_db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "<SA_PASSWORD>" -C < sql/create_identity_and_applezone_tables.sql
docker exec -i applezone_db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "<SA_PASSWORD>" -C < sql/add_name_split_description_product.sql
docker exec -i applezone_db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "<SA_PASSWORD>" -C < sql/add_stockquantity_imageurl_to_product.sql

dotnet run
```

App: `https://localhost:7049`

## Reset Database

```bash
docker-compose down -v
docker-compose up -d
# wait a few seconds, then re-run the 3 sqlcmd commands above
```
