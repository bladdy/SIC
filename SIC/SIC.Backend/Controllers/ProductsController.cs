using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : GenericController<Product>
    {
        public ProductsController(IGenericUnitOfWork<Product> unitOfWork) : base(unitOfWork)
        {

        }
    }
}
