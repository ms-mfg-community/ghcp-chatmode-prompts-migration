---
name: sql-sp-analysis
description: >-
  Analyzes SQL Server stored procedures to extract business logic, map data flows,
  and document dependencies. Use when reverse-engineering stored procedure business
  rules, analyzing SP call chains, or documenting database-layer business logic.
---

## Purpose

Provide specialized knowledge for analyzing SQL Server stored procedures and database objects to extract business logic and support migration decisions.

## Stored Procedure Classification

Categorize each SP by its primary function:

| Category | Indicators | Migration Strategy |
|----------|-----------|-------------------|
| **CRUD** | Simple INSERT/UPDATE/DELETE/SELECT | Replace with EF Core |
| **Business Logic** | CASE, IF/ELSE, calculations, multi-step | Extract to application layer |
| **Reporting** | Complex JOINs, aggregations, CTEs | Keep as SP or move to views |
| **ETL/Batch** | Cursors, bulk operations, temp tables | Evaluate for Azure Data Factory |
| **Security** | Permission checks, row-level security | Move to application middleware |
| **Orchestration** | Calls multiple other SPs | Extract to application service layer |

## Analysis Procedure

### 1. Inventory All Database Objects
```sql
-- List all stored procedures with size
SELECT name, create_date, modify_date, LEN(OBJECT_DEFINITION(object_id)) as char_count
FROM sys.procedures ORDER BY name;

-- List all functions
SELECT name, type_desc FROM sys.objects WHERE type IN ('FN','IF','TF') ORDER BY name;

-- List all views
SELECT name FROM sys.views ORDER BY name;

-- List all triggers
SELECT name, parent_id, OBJECT_NAME(parent_id) as table_name FROM sys.triggers;
```

### 2. Map SP Dependencies
```sql
-- What does this SP reference?
SELECT DISTINCT referenced_entity_name, referenced_minor_name
FROM sys.dm_sql_referenced_entities('dbo.sp_YourProc', 'OBJECT');

-- What references this SP?
SELECT DISTINCT referencing_entity_name
FROM sys.dm_sql_referencing_entities('dbo.sp_YourProc', 'OBJECT');
```

### 3. Identify SP Call Chains
Look for patterns like:
```sql
EXEC dbo.sp_SubProcedure @param1, @param2
EXECUTE @result = dbo.sp_AnotherProc
```

### 4. Business Logic Extraction Patterns

**Decision Logic**:
```sql
-- IF/ELSE blocks → business rules
IF @Status = 'Active' AND @Balance > 0
    SET @Eligible = 1
-- Maps to: "Participant is eligible when status is Active and balance is positive"
```

**Calculations**:
```sql
-- Arithmetic in SET/SELECT → business formulas
SET @Premium = @BaseRate * @AgeFactor * (1 + @RiskAdjustment)
-- Maps to: "Premium = Base Rate × Age Factor × (1 + Risk Adjustment)"
```

**Validation Rules**:
```sql
-- RAISERROR/THROW patterns → validation requirements
IF @DateOfBirth > GETDATE()
    RAISERROR('Date of birth cannot be in the future', 16, 1)
-- Maps to: "Validate: Date of birth must be in the past"
```

**State Transitions**:
```sql
-- UPDATE with WHERE on status → state machine rules
UPDATE Claims SET Status = 'Approved'
WHERE ClaimId = @ClaimId AND Status = 'Pending'
-- Maps to: "Claim can transition from Pending to Approved"
```

## Depth Configuration

### Top-Level Analysis
- Document SP name, parameters, and brief purpose
- Note if it calls other SPs (mark for deep analysis if needed)
- Extract obvious business rules from comments and naming

### Deep Analysis
- Trace complete SP call chains
- Follow dynamic SQL execution (`EXEC(@sql)`, `sp_executesql`)
- Analyze cursor logic and loop patterns
- Map temp table usage and data flows
- Document transaction boundaries (`BEGIN TRAN` / `COMMIT` / `ROLLBACK`)
- Identify error handling patterns (`TRY`/`CATCH`, `@@ERROR`)

## Migration Strategy Recommendations

### Move to EF Core (Recommended for CRUD)
- Simple SELECT/INSERT/UPDATE/DELETE operations
- SPs that map cleanly to entity operations
- SPs with no complex business logic

### Extract to Application Layer (Recommended for Business Logic)
- SPs with complex IF/ELSE/CASE logic
- SPs that implement business rules or calculations
- SPs that enforce workflow/state transitions
- SPs that perform validation

### Keep as Stored Procedures (Recommended for Performance-Critical)
- SPs with complex reporting queries
- SPs that process large datasets
- SPs that use advanced SQL features (window functions, recursive CTEs)
- SPs where the performance cost of moving logic to the app layer is unacceptable

## Output Format

For each SP, document:
1. **Name and purpose** (inferred from code and naming)
2. **Parameters** (name, type, direction, business meaning)
3. **Business rules** (extracted from logic)
4. **Tables touched** (read/write with operation type)
5. **Dependencies** (other SPs, functions, views called)
6. **Complexity** (Simple/Medium/Complex)
7. **Migration recommendation** (EF Core / App Layer / Keep)
8. **Confidence** (High/Medium/Low)
