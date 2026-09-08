using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations;

public class RequirementImageOptionsUnitOfWork : GenericUnitOfWork<RequirementImageOption>, IRequirementImageOptionsUnitOfWork
{
    private readonly IRequirementImageOptionsRepository _repository;

    public RequirementImageOptionsUnitOfWork(IGenericRepository<RequirementImageOption> genericRepository, IRequirementImageOptionsRepository repository)
        : base(genericRepository)
    {
        _repository = repository;
    }

    public async Task<ActionResponse<IEnumerable<RequirementImageOption>>> GetByRequirementIdAsync(int requirementId) => await _repository.GetByRequirementIdAsync(requirementId);
    public async Task<ActionResponse<bool>> SaveOptionsAsync(int requirementId, List<RequirementImageOptionDTO> options) => await _repository.SaveOptionsAsync(requirementId, options);
}
