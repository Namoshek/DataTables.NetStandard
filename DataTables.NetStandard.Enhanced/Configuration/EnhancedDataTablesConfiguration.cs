namespace DataTables.NetStandard.Enhanced.Configuration
{
    public class EnhancedDataTablesConfiguration
    {
        /// <summary>
        /// Global configuration for DataTables filters supported by this package.
        /// </summary>
        public static DataTablesFilterConfiguration FilterConfiguration { get; } = new DataTablesFilterConfiguration();
    }
}
