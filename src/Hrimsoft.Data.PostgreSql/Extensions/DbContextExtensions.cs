using Microsoft.EntityFrameworkCore;

namespace Hrimsoft.Data.PostgreSql
{
    /// <summary>
    /// Extensions of entity framework DbContext
    /// </summary>
    public static class DbContextExtensions
    {
        private const string IN_MEMORY_PROVIDER = "Microsoft.EntityFrameworkCore.InMemory";

        /// <summary>
        /// Check is the current db context is created with InMemory provider
        /// </summary>
        /// <param name="context">tested context</param>
        /// <returns>Returns true if <paramref name="context"/> based on InMemory provider</returns>
        public static bool IsInMemory(this DbContext context)
        {
            if (context == null)
                return false;

            return context.Database.ProviderName == IN_MEMORY_PROVIDER;
        }
    }
}
