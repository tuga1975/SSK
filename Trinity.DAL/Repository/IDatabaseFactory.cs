using Trinity.DAL.DBContext;

namespace Trinity.DAL.Repository
{
    public interface IDatabaseFactory
    {
        SSKEntities GetLocalDbContext();
        TrinityCentralizedDBEntities GetCentralizedDbContext();
    }
}