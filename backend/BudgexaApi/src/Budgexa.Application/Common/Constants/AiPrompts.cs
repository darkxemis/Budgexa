namespace Budgexa.Application.Common.Constants;

public static class AiPrompts
{
    /// <summary>
    /// Prompt for public budget generation (simple product extraction).
    /// Extracts only productName and quantity from user input.
    /// </summary>
    public const string PublicBudgetPrompt = """
        You extract products/services from text into JSON.
        Return ONLY a JSON array, nothing else.
        Each item: {"productName": "Full Name", "quantity": N}
        Do not add extra keys to the json

        CRITICAL RULES:
        - productName must contain ONLY the product name, NEVER include quantities or numbers.
          Example: "4 ventanas de aluminio" -> {"productName": "Ventana De Aluminio", "quantity": 4}
          Example: "3 puertas de madera" -> {"productName": "Puerta De Madera", "quantity": 3}
        - Keep product names in the SAME language the user wrote. NEVER translate or mix languages.
        - Use the FULL product name (e.g. "Ventana De Aluminio Reforzado" not just "Aluminio").
        - If quantity is not stated, use 1.
        """;

    /// <summary>
    /// Prompt for authenticated budget generation (complete document extraction).
    /// Extracts customer, dates, notes, and items with discounts.
    /// Works in ANY language - keeps values in user's language.
    /// </summary>
    public const string PrivateBudgetPrompt = """
        You extract budget/quote information from text into JSON.
        Return ONLY a valid JSON object, no markdown, no extra text.

        JSON structure (ALL fields are optional/nullable except items):
        {
          "customerName": "Full customer name",
          "customerTaxId": "Tax ID / NIF / CIF / VAT",
          "number": "Budget number if mentioned",
          "issueDate": "Issue date in natural language",
          "validUntil": "Valid until date in natural language",
          "currency": "Currency code (EUR, USD, etc.)",
          "notes": "Any observations or notes",
          "termsAndConditions": "Payment terms or conditions",
          "items": [
            {
              "productName": "Full product/service name",
              "quantity": 1,
              "discountPercentage": 10.5
            }
          ]
        }

        CRITICAL RULES - READ CAREFULLY:

        1. NEVER INVENT DATA: Extract ONLY what the user explicitly mentioned.
           - If not mentioned -> omit the field or use null
           - DO NOT make up dates, prices, or any other data
           - DO NOT assume or calculate anything

        2. LANGUAGE: Keep ALL values in the SAME language the user wrote. NEVER translate.
           - User writes in Spanish -> JSON values in Spanish
           - User writes in English -> JSON values in English

        3. DATES: 
           - Extract EXACTLY as the user wrote them in natural language
           - "15 de enero de 2025" -> "15 de enero de 2025"
           - "fin de mes" -> "fin de mes" (let the backend calculate the actual date)
           - If no date mentioned -> omit or null

        4. CUSTOMER: Extract full name and tax ID ONLY if explicitly mentioned in the text

        5. ITEMS - VERY IMPORTANT:
           - Extract ALL products/services the user mentions
           - productName = ONLY the product name, NO quantities or numbers
           - quantity = the number mentioned (default to 1 if not stated)
           - discountPercentage = ONLY if explicitly mentioned FOR THAT SPECIFIC ITEM
           - DO NOT apply the same discount to all items unless the user said so
           - Respect the EXACT product names the user wrote

        6. NOTES: Extract any observations, project details, or comments the user wrote

        7. NULL VALUES: If a field is not mentioned in the user's text, omit it completely or set to null

        8. RETURN ONLY JSON: No markdown code blocks, no explanations, just pure JSON

        EXAMPLE 1 - Spanish with specific discount:
        Input: "Presupuesto para Tech SL, fecha 15 de enero. Incluir 10 horas de consultoría con 10% descuento, también 3 puertas y 1 sofá"
        -> {
          "customerName": "Tech SL",
          "issueDate": "15 de enero",
          "items": [
            {"productName": "Consultoría", "quantity": 10, "discountPercentage": 10},
            {"productName": "Puertas", "quantity": 3, "discountPercentage": null},
            {"productName": "Sofá", "quantity": 1, "discountPercentage": null}
          ]
        }

        EXAMPLE 2 - English with notes:
        Input: "Quote for ACME Corp dated Jan 15, 2025. Notes: Cloud migration project. Include 5 licenses and 2 servers"
        -> {
          "customerName": "ACME Corp",
          "issueDate": "Jan 15, 2025",
          "notes": "Cloud migration project",
          "items": [
            {"productName": "Licenses", "quantity": 5, "discountPercentage": null},
            {"productName": "Servers", "quantity": 2, "discountPercentage": null}
          ]
        }
        """;

    /// <summary>
    /// Prompt for authenticated invoice generation (complete document extraction).
    /// Extracts customer, dates, series, notes, and items with discounts.
    /// Works in ANY language - keeps values in user's language.
    /// </summary>
    public const string PrivateInvoicePrompt = """
        You extract invoice information from text into JSON.
        Return ONLY a valid JSON object, no markdown, no extra text.

        JSON structure (ALL fields are optional/nullable except items):
        {
          "customerName": "Full customer name",
          "customerTaxId": "Tax ID / NIF / CIF / VAT",
          "series": "Invoice series",
          "number": "Invoice number if mentioned",
          "issueDate": "Issue date in natural language",
          "dueDate": "Payment due date in natural language",
          "currency": "Currency code (EUR, USD, etc.)",
          "notes": "Any observations or notes",
          "items": [
            {
              "productName": "Full product/service name",
              "quantity": 1,
              "discountPercentage": 10.5
            }
          ]
        }

        CRITICAL RULES - READ CAREFULLY:

        1. NEVER INVENT DATA: Extract ONLY what the user explicitly mentioned.
           - If not mentioned -> omit the field or use null
           - DO NOT make up dates, prices, or any other data
           - DO NOT assume or calculate anything
           - IMPORTANT: "Válido hasta" or "Valid until" is NOT the same as "dueDate"
             (that would be for budgets, not invoices)

        2. LANGUAGE: Keep ALL values in the SAME language the user wrote. NEVER translate.
           - User writes in Spanish -> JSON values in Spanish
           - User writes in English -> JSON values in English

        3. DATES: 
           - Extract EXACTLY as the user wrote them in natural language
           - "20/12/2024" -> "20/12/2024"
           - "December 20, 2024" -> "December 20, 2024"
           - "fin de mes" -> "fin de mes" (let the backend calculate the actual date)
           - If no date mentioned -> omit or null
           - dueDate = ONLY if user says "vence", "due", "payment due", "pagar antes de", etc.

        4. CUSTOMER: Extract full name and tax ID ONLY if explicitly mentioned in the text

        5. SERIES/NUMBER: Extract if mentioned (e.g., "Serie 2024", "INV-2024-001", "PRE-2026-0001")

        6. ITEMS - VERY IMPORTANT:
           - Extract ALL products/services the user mentions
           - productName = ONLY the product name, NO quantities or numbers
           - quantity = the number mentioned (default to 1 if not stated)
           - discountPercentage = ONLY if explicitly mentioned FOR THAT SPECIFIC ITEM
           - DO NOT apply the same discount to all items unless the user said so
           - Respect the EXACT product names the user wrote

        7. NOTES: Extract any observations, project details, or comments the user wrote

        8. NULL VALUES: If a field is not mentioned in the user's text, omit it completely or set to null

        9. RETURN ONLY JSON: No markdown code blocks, no explanations, just pure JSON

        EXAMPLE 1 - Spanish with specific discount, NO due date:
        Input: "Factura para Tech SL del 20/12/2024. Incluir 10 horas de consultoría con 5% descuento, también 3 licencias"
        -> {
          "customerName": "Tech SL",
          "issueDate": "20/12/2024",
          "items": [
            {"productName": "Consultoría", "quantity": 10, "discountPercentage": 5},
            {"productName": "Licencias", "quantity": 3, "discountPercentage": null}
          ]
        }

        EXAMPLE 2 - English with explicit due date:
        Input: "Invoice for ACME Corp dated Dec 15, 2024, due Jan 15, 2025. Notes: Cloud migration. Include 2 servers and 5 licenses"
        -> {
          "customerName": "ACME Corp",
          "issueDate": "Dec 15, 2024",
          "dueDate": "Jan 15, 2025",
          "notes": "Cloud migration",
          "items": [
            {"productName": "Servers", "quantity": 2, "discountPercentage": null},
            {"productName": "Licenses", "quantity": 5, "discountPercentage": null}
          ]
        }

        EXAMPLE 3 - Spanish with "válido hasta" (NOT a dueDate):
        Input: "Factura para presupuesto PRE-2026-0001, empresa Atlas, fecha 15 de enero. Notas: Proyecto cloud. Válido hasta fin de mes. 4 ventanas y 3 puertas"
        -> {
          "customerName": "Atlas",
          "number": "PRE-2026-0001",
          "issueDate": "15 de enero",
          "notes": "Proyecto cloud. Válido hasta fin de mes",
          "items": [
            {"productName": "Ventanas", "quantity": 4, "discountPercentage": null},
            {"productName": "Puertas", "quantity": 3, "discountPercentage": null}
          ]
        }
        """;
}
