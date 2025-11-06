using System.Threading.Tasks;

namespace ECommerce519.APIV9.Repositories
{
    public class ProductColorRepository : Repository<ProductColor>, IProductColorRepository
    {
        private ApplicationDbContext _context;// = new();

        public ProductColorRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public void RemoveRange(IEnumerable<ProductColor> productColors)
        {
            _context.RemoveRange(productColors);
        }
    }
}
