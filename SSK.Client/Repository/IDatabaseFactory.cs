
using SSK.DbContext;

namespace SSK.Client.Repository
{
    public interface IDatabaseFactory
    {
        SSKEntities GetDbContext();
    }
}