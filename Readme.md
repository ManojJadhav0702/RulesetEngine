# 🧩 RulesetEngine

## Overview

**RulesetEngine** is a modular and extensible **.NET 9.0** solution designed to evaluate and apply dynamic business rules.  
It follows a **clean architecture pattern**, separating API, Application, Domain, and Data layers to ensure **scalability, maintainability, and testability**.

---

## 📁 Project Structure

```
RulesetEngine/
│
├── docs/                      # Documentation and design notes
├── script/                    # Setup and deployment scripts
├── src/
│   ├── RulesetEngine.API/         # ASP.NET Core Web API (entry point)
│   ├── RulesetEngine.Application/ # Business logic and rule orchestration
│   ├── RulesetEngine.Data/        # Data access layer
│   └── RulesetEngine.Domain/      # Core domain models and entities
│
├── tests/
│   ├── RulesetEngine.Tests/            # Unit tests
│   └── RulesetEngine.IntegrationTests/ # Integration tests
│
└── RulesetEngine.sln
```

---

## ⚙️ Tech Stack

- .NET 9.0  
- C# 12  
- Entity Framework Core  
- xUnit / MSTest (for tests)  
- Swagger / OpenAPI (for API documentation)

---

## 🚀 Setup Instructions

### 1️⃣ Prerequisites

- .NET SDK 9.0+  
- SQL Server / LocalDB or other configured database  
- Visual Studio 2022 or VS Code  

---

### download the Zip from attachemnt and extract it.

```bash
git clone https://github.com/manojjadhav-dev/RulesetEngine.git
cd RulesetEngine
dotnet restore
```

---

### Database Configuration

Update the `appsettings.json` file in `src/RulesetEngine.API` with your connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=RulesetDB;Trusted_Connection=True;"
}
```

### Database Setup

#### Run Initial Script Manually (Recommended for First Run)

Before starting the API, **create and seed your database manually** using the provided SQL script.

1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**.  
2. Create a new database and Execute the script file located at:
   ```
   script/initial_script.sql
   ```
   This will:
   - Create the schema (`Rulesets`, `Rules`, `Conditions`, `EvaluationLogs`)
   - Set up foreign keys and indexes
   - Insert sample data for testing (e.g., *Ruleset One* & *Ruleset Two*)
4. Verify setup:
   ```sql
   SELECT * FROM Rulesets;
   SELECT * FROM Rules;
   SELECT * FROM Conditions;
   ```
   You should see:
   - Two active rulesets (`Ruleset One`, `Ruleset Two`)
   - Rules and conditions mapped correctly

---

### Run the Application

```bash
cd src/RulesetEngine.API
dotnet run
```

Access the API at:  
👉 [https://localhost:5001/swagger](https://localhost:5001/swagger)  
or  
👉 [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🧪 Running Tests

To execute all unit and integration tests:

```bash
dotnet test
```

---

## 🏗️ Project Architecture

| Layer | Description |
|--------|-------------|
| **Domain** | Contains core business entities, value objects, and domain logic. |
| **Application** | Implements use cases, validation, and business orchestration. |
| **Data** | Handles persistence, repositories, and EF Core context. |
| **API** | Exposes endpoints and integrates the Application layer via controllers. |

---

## 🧠 Example Endpoint

**POST** `https://localhost:5002/api/evaluate`
Evaluates a rule set against a given order or input object.

**Request Example:**
```json
{
  "orderId": "1245101",
  "publisherNumber": "99990",
  "publisherName": "BookWorld Ltd",
  "orderMethod": "POD",
  "shipments": [
    {
      "shipTo": { "isoCountry": "US" }
    }
  ],
  "items": [
    {
      "sku": "PB-001",
      "printQuantity": 10,
      "components": [
        {
          "code": "Cover",
          "attributes": { "BindTypeCode": "PB" }
        },
        {
          "code": "Content",
          "attributes": { "BindTypeCode": "PB" }
        }
      ]
    }
  ]
}

```

**Response Example:**
```json
{
  "matched": true,
  "productionPlant": "US",
  "matchedRuleset": "Ruleset One",
  "matchedRule": "Rule 1",
  "reason": "BindTypeCode=PB, IsCountry=US, PrintQuantity=10",
  "evaluationTimeMs": 3
}

```
**Curl Bash 8*
```
curl -X 'POST' \
  'https://localhost:5002/api/evaluate' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "orderId": "1245101",
  "publisherNumber": "99990",
  "publisherName": "BookWorld Ltd",
  "orderMethod": "POD",
  "shipments": [
    {
      "shipTo": { "isoCountry": "US" }
    }
  ],
  "items": [
    {
      "sku": "PB-001",
      "printQuantity": 10,
      "components": [
        {
          "code": "Cover",
          "attributes": { "BindTypeCode": "PB" }
        },
        {
          "code": "Content",
          "attributes": { "BindTypeCode": "PB" }
        }
      ]
    }
  ]
}
'

```
---

## 🧾 Testing & Validation

- Integration tests validate rule execution end-to-end.  
- Unit tests verify individual rule evaluation logic.  
- CI/CD pipelines (optional) can run `dotnet test` automatically.

---

## 🔮 Future Enhancements

- Add dynamic rule creation via configuration or database.  
- Extend support for external data sources.  
- Introduce caching and performance tuning.  
- Enhance logging and monitoring through **Serilog** or **Application Insights**.  

---

📘 **Author:** Manoj Jadhav  
