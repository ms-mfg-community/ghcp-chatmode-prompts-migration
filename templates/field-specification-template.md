# Field Specification Template

Use this template when documenting field-level specifications during Phase 0 discovery. Every form field, data entry point, and UI control should be documented using this structure.

## Template

```markdown
#### [Field Label as displayed in UI]

| Property | Value |
|----------|-------|
| **Field Name** | [Code/database column name] |
| **Display Label** | [User-facing label text] |
| **Data Type** | [string / int / decimal / date / bool / enum / etc.] |
| **Max Length** | [Maximum character/digit length, or N/A] |
| **Required** | [Yes / No / Conditional — explain condition] |
| **Default Value** | [Default value if any, or None] |
| **Validation Rules** | [List all validation rules] |
| **Format/Mask** | [Input mask or display format, e.g., MM/DD/YYYY, (###) ###-####] |
| **Allowed Values** | [Enum values, dropdown options, or range constraints] |
| **Tooltip/Help Text** | [Tooltip text shown to users, or suggested help text] |
| **Behavior on Save** | [What happens when the record is saved — transformations, triggers, side effects] |
| **Behavior on Change** | [What happens when the field value changes — cascading updates, recalculations] |
| **Read-Only Conditions** | [When is this field read-only or disabled?] |
| **Visibility Conditions** | [When is this field hidden or shown?] |
| **Source** | [Code file and line number where this field is defined/used] |
| **Database Column** | [Table.Column mapping in the database] |
| **Confidence** | [High / Medium / Low — how confident is the extraction?] |
| **Notes** | [Any additional context, edge cases, or unknowns] |
```

## Example

```markdown
#### Date of Birth

| Property | Value |
|----------|-------|
| **Field Name** | txtDateOfBirth |
| **Display Label** | Date of Birth |
| **Data Type** | date |
| **Max Length** | 10 |
| **Required** | Yes |
| **Default Value** | None |
| **Validation Rules** | Must be a valid date; must be in the past; participant must be >= 18 years old |
| **Format/Mask** | MM/DD/YYYY |
| **Allowed Values** | Any valid past date where age >= 18 |
| **Tooltip/Help Text** | "Enter the participant's date of birth" |
| **Behavior on Save** | Calculates and stores age in `CalculatedAge` column; triggers eligibility check SP |
| **Behavior on Change** | Recalculates age display; updates eligibility status indicator |
| **Read-Only Conditions** | Read-only when record status = "Approved" |
| **Visibility Conditions** | Always visible |
| **Source** | `EnrollmentForm.cs:142`, `EnrollmentForm.Designer.cs:87` |
| **Database Column** | `Participants.DateOfBirth` |
| **Confidence** | High |
| **Notes** | Age calculation uses SQL `DATEDIFF` in `sp_CalculateEligibility` — verify leap year handling |
```

## Usage Notes

- Use `FS-[NNN]` numbering for cross-referencing from user stories (e.g., FS-001, FS-002)
- Group fields by form/screen, then by section within the form
- For grid/table columns, document each column as a field with the grid as context
- For dropdown/combo fields, list all allowed values or reference the lookup table
- For calculated fields, document the full calculation formula
- Mark confidence as LOW for any field where behavior was inferred rather than directly observed in code
