using SIC.Shared.Enums;

namespace SIC.Shared.DTOs;

public class UpdateDesignStatusDTO
{
    public DesignStatus DesignStatus { get; set; } = DesignStatus.Pending;
}
