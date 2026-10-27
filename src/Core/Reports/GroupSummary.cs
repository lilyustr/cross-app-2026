namespace Core.Reports;

/// <summary>
/// DTO зведеної групи для відображення підсумкових показників. (для зведеного рядка за групою)
/// </summary>
public sealed record GroupSummary(
    string Key,
    int Count,
    int TotalQuantity
);