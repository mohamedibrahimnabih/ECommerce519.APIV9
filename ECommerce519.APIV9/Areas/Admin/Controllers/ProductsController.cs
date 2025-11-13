using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce519.APIV9.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;// = new();
        private readonly IProductRepository _productRepository;
        private readonly IRepository<Category> _categoryRepository;// = new();
        private readonly IRepository<Brand> _brandRepository;// = new();
        private readonly IRepository<ProductSubImage> _productSubImageRepository;// = new();
        private readonly IProductColorRepository _productColorRepository;// = new();
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, IProductRepository productRepository, IRepository<Category> categoryRepository, IRepository<Brand> brandRepository, IRepository<ProductSubImage> productSubImageRepository, IProductColorRepository productColorRepository, ILogger<ProductsController> logger)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productSubImageRepository = productSubImageRepository;
            _productColorRepository = productColorRepository;
            _logger = logger;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> GetAll(FilterProductRequest filterProductRequest, CancellationToken cancellationToken, [FromQuery] int page = 1)
        {
            const decimal discount = 50;
            var products = await _productRepository.GetAsync(includes: [e => e.Category, e => e.Brand], tracked: false, cancellationToken: cancellationToken);

            #region Filter Product
            FilterProductResponse filterProductResponse = new();

            // Add Filter 
            if (filterProductRequest.name is not null)
            {
                products = products.Where(e => e.Name.Contains(filterProductRequest.name.Trim()));
                filterProductResponse.Name = filterProductRequest.name;
            }

            if (filterProductRequest.minPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * e.Discount / 100 > filterProductRequest.minPrice);
                filterProductResponse.MinPrice = filterProductRequest.minPrice;
            }

            if (filterProductRequest.maxPrice is not null)
            {
                products = products.Where(e => e.Price - e.Price * e.Discount / 100 < filterProductRequest.maxPrice);
                filterProductResponse.MaxPrice = filterProductRequest.maxPrice;
            }

            if (filterProductRequest.categoryId is not null)
            {
                products = products.Where(e => e.CategoryId == filterProductRequest.categoryId);
                filterProductResponse.CategoryId = filterProductRequest.categoryId;
            }

            if (filterProductRequest.brandId is not null)
            {
                products = products.Where(e => e.BrandId == filterProductRequest.brandId);
                filterProductResponse.BrandId = filterProductRequest.brandId;
            }

            if (filterProductRequest.lessQuantity)
            {
                products = products.OrderBy(e => e.Quantity);
                filterProductResponse.LessQuantity = filterProductRequest.lessQuantity;
            }

            #endregion

            #region Pagination
            PaginationResponse paginationResponse = new();

            // Pagination
            paginationResponse.TotalPages = Math.Ceiling(products.Count() / 8.0);
            paginationResponse.CurrentPage = page;
            products = products.Skip((page - 1) * 8).Take(8); // 0 .. 8 
            #endregion

            return Ok(new
            {
                Products = products.AsEnumerable(),
                FilterProductResponse = filterProductResponse,
                PaginationResponse = paginationResponse
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id, includes: [e => e.productSubImages, e => e.ProductColors], tracked: false, cancellationToken: cancellationToken);

            if (product is null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(CreateProductRequest createProductRequest, CancellationToken cancellationToken)
        {
            var transaction = _context.Database.BeginTransaction();

            Product product = createProductRequest.Adapt<Product>();

            try
            {
                if (createProductRequest.Img is not null && createProductRequest.Img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createProductRequest.Img.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await createProductRequest.Img.CopyToAsync(stream);
                    }

                    // Save Img in db
                    product.MainImg = fileName;
                }

                // Save product in db
                var productCreated = await _productRepository.AddAsync(product, cancellationToken: cancellationToken);
                await _productRepository.CommitAsync(cancellationToken);

                if (createProductRequest.SubImgs is not null && createProductRequest.SubImgs.Count > 0)
                {
                    foreach (var item in createProductRequest.SubImgs)
                    {
                        // Save Img in wwwroot
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            await item.CopyToAsync(stream);
                        }

                        await _productSubImageRepository.AddAsync(new()
                        {
                            Img = fileName,
                            ProductId = productCreated.Id,
                        }, cancellationToken: cancellationToken);
                    }

                    await _productSubImageRepository.CommitAsync(cancellationToken);
                }

                if (createProductRequest.Colors is not null && createProductRequest.Colors.Any())
                {
                    foreach (var item in createProductRequest.Colors)
                    {
                        await _productColorRepository.AddAsync(new()
                        {
                            Color = item,
                            ProductId = productCreated.Id,
                        }, cancellationToken: cancellationToken);
                    }

                    await _productColorRepository.CommitAsync(cancellationToken);
                }

                transaction.Commit();

                return CreatedAtAction(nameof(GetOne), new { id = product.Id }, new
                {
                    success_notifaction = "Add Product Successfully"
                });
            }
            catch (Exception ex)
            {
                // Logging
                _logger.LogError(ex.Message);
                transaction.Rollback();

                // Validation
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Error While Saving the product",
                    Description = ex.Message,
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id, UpdateProductRequest updateProductRequest, CancellationToken cancellationToken)
        {
            var productInDb = await _productRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (productInDb is null)
                return NotFound();

            if (updateProductRequest.Img is not null)
            {
                if (updateProductRequest.Img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateProductRequest.Img.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await updateProductRequest.Img.CopyToAsync(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_images", productInDb.MainImg);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    // Save Img in db
                    productInDb.MainImg = fileName;
                }
            }

            productInDb.Name = updateProductRequest.Name;
            productInDb.Description = updateProductRequest.Description;
            productInDb.Status = updateProductRequest.Status;
            productInDb.Discount = updateProductRequest.Discount;
            productInDb.Price = updateProductRequest.Price;
            productInDb.Quantity = updateProductRequest.Quantity;
            productInDb.CategoryId = updateProductRequest.CategoryId;
            productInDb.BrandId = updateProductRequest.BrandId;

            _productRepository.Update(productInDb);
            await _productRepository.CommitAsync(cancellationToken);

            if (updateProductRequest.SubImgs is not null && updateProductRequest.SubImgs.Count > 0)
            {
                var oldProductSubImages = await _productSubImageRepository.GetAsync(e => e.ProductId == id);

                foreach (var item in oldProductSubImages)
                {
                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", item.Img);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    _productSubImageRepository.Delete(item);
                }

                foreach (var item in updateProductRequest.SubImgs)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await item.CopyToAsync(stream);
                    }

                    await _productSubImageRepository.AddAsync(new()
                    {
                        Img = fileName,
                        ProductId = id,
                    }, cancellationToken);
                }

                await _productSubImageRepository.CommitAsync(cancellationToken);
            }


            if (updateProductRequest.Colors is not null && updateProductRequest.Colors.Any())
            {

                var oldProductColors = await _productColorRepository.GetAsync(e => e.ProductId == id);

                _productColorRepository.RemoveRange(oldProductColors);

                foreach (var item in updateProductRequest.Colors)
                {
                    await _productColorRepository.AddAsync(new()
                    {
                        Color = item,
                        ProductId = id,
                    }, cancellationToken);
                }

                await _productColorRepository.CommitAsync(cancellationToken);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetOneAsync(e => e.Id == id, includes: [e => e.productSubImages, e => e.ProductColors], cancellationToken: cancellationToken);

            if (product is null)
                return NotFound();

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\product_images", product.MainImg);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            foreach (var item in product.productSubImages)
            {
                var subImgOldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", item.Img);
                if (System.IO.File.Exists(subImgOldPath))
                    System.IO.File.Delete(subImgOldPath);
            }

            _productRepository.Delete(product);
            await _productRepository.CommitAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{productId}/{img}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> DeleteSubImg(int productId, string img, CancellationToken cancellationToken)
        {
            var productSubImgInDb = await _productSubImageRepository.GetOneAsync(e => e.ProductId == productId && e.Img == img);

            if (productSubImgInDb is null)
                return NotFound();

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\product_images", productSubImgInDb.Img);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            _productSubImageRepository.Delete(productSubImgInDb);
            await _productSubImageRepository.CommitAsync(cancellationToken);

            return NoContent();
        }
    }
}
