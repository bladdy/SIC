using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : GenericController<Item>
{
    public ItemsController(IGenericUnitOfWork<Item> unitOfWork) : base(unitOfWork)
    {
    }
}