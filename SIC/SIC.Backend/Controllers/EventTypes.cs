using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventTypesController : GenericController<EventType>
{
    public EventTypesController(IGenericUnitOfWork<EventType> unitOfWork) : base(unitOfWork)
    {
    }
}
