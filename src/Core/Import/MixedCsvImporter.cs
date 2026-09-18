namespace Core.Import;

using System.Globalization;
using Core.Dto;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<IStorageRecord> Load(string path)
    {
        var items = new List<IStorageRecord>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<IStorageRecord>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Розпізнавання товару за префіксом "P"
            ["P", var id, var sku, var name, var unit, var qty]
                when int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) && q >= 0 =>
                new ParseOk(new ProductDto(id, sku, name, unit, q)),

            ["P", _, "", _, _, _] or ["P", _, _, "", _, _] =>
                new ParseFailed("SKU або назва товару порожні"),

            ["P", ..] =>
                new ParseFailed("Некоректний формат рядка товару (P)"),

            // Розпізнавання складу за префіксом "W"
            ["W", var id, var code, var address, var cap]
                when int.TryParse(cap, NumberStyles.Integer, CultureInfo.InvariantCulture, out int c) && c >= 0 =>
                new ParseOk(new WarehouseDto(id, code, address, c)),

            ["W", ..] =>
                new ParseFailed("Некоректний формат рядка складу (W)"),

            // Невідомий префікс
            [var prefix, ..] =>
                new ParseFailed($"Невідомий префікс сутності: '{prefix}'"),

            _ =>
                new ParseFailed("Порожній або пошкоджений рядок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(IStorageRecord Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}