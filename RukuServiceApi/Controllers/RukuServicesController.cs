using RukuServiceApi.Context;
using RukuServiceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RukuServiceApi.Controllers;

public class RukuServicesController(
    ApplicationDbContext context,
    ILogger<RukuServicesController> logger
) : BaseController<RukuService, ApplicationDbContext>(context, logger)
{
    protected override object GetEntityId(RukuService entity) => entity.Id;

    public override async Task<ActionResult<RukuService>> Create([FromBody] RukuService entity)
    {
        bool duplicateService = await _context.RukuServices.AnyAsync(ps =>
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

    public override async Task<ActionResult<RukuService>> Update(int id, [FromBody] RukuService entity)
    {
        bool duplicateService = await _context.RukuServices.AnyAsync(ps =>
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
