using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces;

public interface IRequirementImageOptionsRepository : IGenericRepository<RequirementImageOption>
{
    Task<ActionResponse<IEnumerable<RequirementImageOption>>> GetByRequirementIdAsync(int requirementId);
    Task<ActionResponse<bool>> SaveOptionsAsync(int requirementId, List<RequirementImageOptionDTO> options);
}
