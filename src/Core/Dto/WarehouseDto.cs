namespace Core.Dto;

public record WarehouseDto(
    string Id,
    string Code,
    string Address,
    int Capacity) : IStorageRecord;