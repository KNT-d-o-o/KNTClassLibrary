namespace KNTCommon.Business.Interface
{
    public interface IApplicationValidatorRepository
    {        
        public List<string> EfTableValidation<TContext>(params string[] excludeTables) where TContext : class, IDisposable, new();
        List<string> FindCorruptData();

    }
}
