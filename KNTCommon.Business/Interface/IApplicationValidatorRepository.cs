using Microsoft.EntityFrameworkCore;
using KNTCommon.Business.Repositories;

namespace KNTCommon.Business.Interface
{
    public interface IApplicationValidatorRepository
    {        
        public List<string> EfTableValidation<TContext>(params string[] excludeTables) where TContext : class, IDisposable, new();
        List<string> FindCorruptData();
        List<string> DbEfTableValidation<TContext>(params string[] excludeTables) where TContext : DbContext, IDisposable, new();        
        IEnumerable<ValidatorMissingTranslation> GetMissingEntityTranslation(params string[] excludeTables);

    }
}
