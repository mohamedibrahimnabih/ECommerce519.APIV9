using Microsoft.EntityFrameworkCore;

namespace ECommerce519.APIV9.Repositories.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task AddRangeAsync(IEnumerable<Product> products, CancellationToken cancellationToken = default);
    }
}
