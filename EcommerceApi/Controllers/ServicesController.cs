using EcommerceApi.Context;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Controllers
{
    public class ServicesController(
        ApplicationDbContext context,
        ILogger<ServicesController> logger
    ) : BaseController<Service, ApplicationDbContext>(context, logger)
    {
        protected override object GetEntityId(Service entity) => entity.Id;

        public override async Task<ActionResult<Service>> Create([FromBody] Service entity)
        {
            bool duplicateService = await _context.Services.AnyAsync(ps =>
                ps.Title == entity.Title || ps.Description == entity.Description
            );
            if (duplicateService)
            {
                return Conflict(
                    new
                    {
                        message = $"Service with title '{entity.Title}' or description '{entity.Description}' already exists.",
                        code = "DUPLICATE_SERVICE",
                    }
                );
            }
            return await base.Create(entity);
        }

        public override async Task<ActionResult<Service>> Update(int id, [FromBody] Service entity)
        {
            bool duplicateService = await _context.Services.AnyAsync(ps =>
                ps.Id != id && (ps.Title == entity.Title || ps.Description == entity.Description)
            );
            if (duplicateService)
            {
                return Conflict(
                    new
                    {
                        message = $"Service with title '{entity.Title}' or description '{entity.Description}' already exists.",
                        code = "DUPLICATE_SERVICE",
                    }
                );
            }
            return await base.Update(id, entity);
        }
    }
}
