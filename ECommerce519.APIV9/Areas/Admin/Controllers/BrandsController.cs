using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce519.APIV9.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IRepository<Brand> _brandRepository;

        public BrandsController(IRepository<Brand> brandRepository)
        {
            _brandRepository = brandRepository;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var brands = await _brandRepository.GetAsync(tracked: false, cancellationToken: cancellationToken);

            // Add Filter

            return Ok(brands.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.Status,
            }).AsEnumerable());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken, tracked: false);

            if (brand is null)
                return NotFound();

            //return View(new UpdateBrandVM()
            //{
            //    Id = brand.Id,
            //    Name = brand.Name,
            //    Description = brand.Description,
            //    Status = brand.Status,
            //    Img = brand.Img,
            //});

            return Ok(brand);
        }

        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(CreateBrandRequest createBrandRequest, CancellationToken cancellationToken)
        {
            Brand brand = createBrandRequest.Adapt<Brand>();

            if (createBrandRequest.Img is not null && createBrandRequest.Img.Length > 0)
            {
                // Save Img in wwwroot
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createBrandRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await createBrandRequest.Img.CopyToAsync(stream);
                }

                // Save Img in db
                brand.Img = fileName;
            }

            // Save brand in db
            await _brandRepository.AddAsync(brand, cancellationToken);
            await _brandRepository.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetOne), new { id = brand.Id }, new
            {
                success_notifaction = "Add Brand Successfully"
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id, UpdateBrandRequest updateBrandRequest, CancellationToken cancellationToken)
        {
            var brandInDb = await _brandRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (brandInDb is null)
                return NotFound();

            if (updateBrandRequest.NewImg is not null)
            {
                if (updateBrandRequest.NewImg.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateBrandRequest.NewImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await updateBrandRequest.NewImg.CopyToAsync(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images", brandInDb.Img);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    // Save Img in db
                    brandInDb.Img = fileName;
                }
            }

            brandInDb.Name = updateBrandRequest.Name;
            brandInDb.Description = updateBrandRequest.Description;
            brandInDb.Status = updateBrandRequest.Status;

            await _brandRepository.CommitAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.GetOneAsync(e => e.Id == id);

            if (brand is null)
                return NotFound();

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\brand_images", brand.Img);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            _brandRepository.Delete(brand);
            await _brandRepository.CommitAsync(cancellationToken);

            return NoContent();
        }
    }
}
