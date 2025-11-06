using Microsoft.EntityFrameworkCore;

namespace ECommerce519.APIV9.Repositories.IRepositories
{
    public interface IProductColorRepository : IRepository<ProductColor>
    {
        void RemoveRange(IEnumerable<ProductColor> productColors);
    }
}
