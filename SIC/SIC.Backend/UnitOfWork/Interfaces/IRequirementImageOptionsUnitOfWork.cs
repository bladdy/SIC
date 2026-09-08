using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces;

public interface IRequirementImageOptionsUnitOfWork : IGenericUnitOfWork<RequirementImageOption>
{
    Task<ActionResponse<IEnumerable<RequirementImageOption>>> GetByRequirementIdAsync(int requirementId);
    Task<ActionResponse<bool>> SaveOptionsAsync(int requirementId, List<RequirementImageOptionDTO> options);
}
