using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Tc.Psg.LookupExportUtility
{
    public class ScriptRunner
    {
        private SqlConnectionStringBuilder _scsb;

        public ScriptRunner(string server, string database, string username, string password)
        {
            _scsb = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = username,
                Password = password
            };
        }

        public DataTable RunScript(string scriptName, params SqlParameter[] parameters)
        {
            string sql;
            DataTable table = new DataTable();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Tc.Psg.LookupExportUtility.{scriptName}"))
            using (StreamReader reader = new StreamReader(stream))
            {
                sql = reader.ReadToEnd();
            }

            using (SqlConnection connection = new SqlConnection(_scsb.ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();

                foreach (SqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }
            }

            return table;
        }
    }
}
