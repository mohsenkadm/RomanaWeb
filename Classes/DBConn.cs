namespace RomanaWeb.Classes
{
    public enum ConnectionType
    {
        SqlServerLocal,
        SqlServerSmarter
    }

    public static class DBConn
    {
        public static readonly ConnectionType ConnectionType = ConnectionType.SqlServerSmarter;

        public static string ConnectionString
        {
            get
            {
                return ConnectionType switch
                {
                    ConnectionType.SqlServerLocal => "Server=.; Database=RomanaDb; Trusted_Connection=True; " +
                    " TrustServerCertificate=True",
                    ConnectionType.SqlServerSmarter => "Data Source=SQL5111.site4now.net;Initial" +
                    " Catalog=db_a9f134_romanadb;User Id=db_a9f134_romanadb_admin;Password=pkQv1bE3Q57GO0n; MultipleActiveResultSets=True;",
                    _ => "",
                };
            }
        }
    }
}
