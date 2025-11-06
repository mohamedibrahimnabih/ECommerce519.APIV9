using System.Threading.Tasks;

namespace ECommerce519.APIV9.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _context;// = new();

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default)
        {
            await _context.AddRangeAsync(products, cancellationToken);
        }
    }
}
