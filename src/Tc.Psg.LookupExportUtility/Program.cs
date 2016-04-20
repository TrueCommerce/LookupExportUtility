using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Tc.Psg.LookupExportUtility
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Transaction Manager Lookup Export Utility");
            Console.WriteLine("v1.0.0");
            Console.WriteLine();

            Console.Write(@"What is the name of your Transaciton Manager SQL Server? ");
            string sqlServer = Console.ReadLine();

            Console.Write("What is the name of your Transaction Manager database (usually \"Integrator\")? ");
            string sqlDatabase = Console.ReadLine();

            Console.Write("What is your SQL Server username? ");
            string sqlUser = Console.ReadLine();

            Console.Write("What is your SQL Server password? ");
            string sqlPassword = Console.ReadLine();

            Console.Write("Connecting to Transaction Manager database... ");

            ScriptRunner scriptRunner = new ScriptRunner(sqlServer, sqlDatabase, sqlUser, sqlPassword);
            DataTable bspResults = scriptRunner.RunScript("ListPlugins.sql");

            Console.WriteLine("Done!");

            int pluginId = -1;

            Console.WriteLine();

            for (int i = 0; i < bspResults.Rows.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {bspResults.Rows[i]["BSPName"]}");
            }

            Console.WriteLine();
            Console.Write("Which BSP do you want to export lookups for (number only, please)? ");
            string bspSelection = Console.ReadLine();
            pluginId = (int)bspResults.Rows[int.Parse(bspSelection) - 1]["BspPlugInID"];

            Console.Write("Where should files be exported? ");
            string exportFolder = Console.ReadLine();

            if (!Directory.Exists(exportFolder))
            {
                Directory.CreateDirectory(exportFolder);
                Console.WriteLine($"Created {exportFolder}.");
            }

            Console.Write("Obtaining lookup table data... ");
            DataTable lookupData = scriptRunner.RunScript("LookupExport.sql", new SqlParameter("@pluginId", pluginId));
            Console.WriteLine("Done!");

            string filename = string.Empty;
            StringBuilder contents = new StringBuilder();

            Console.Write("Exporting data... ");
            foreach (DataRow row in lookupData.Rows)
            {
                string newFileName = SanitizeFileName($"{row["BSPName"]}_{row["LookupName"]}_{row["LookupType"]}Level_{row["LookupPartner"] ?? "Lookup"}.txt");

                if (!newFileName.Equals(filename))
                {
                    if (contents.Length > 0)
                    {
                        File.WriteAllText(Path.Combine(exportFolder, filename), contents.ToString());
                    }

                    filename = newFileName;
                    contents = new StringBuilder("InternalValue,ExternalValue\r\n");
                }

                contents.Append($"\"{row["InternalValue"]}\",\"{row["ExternalValue"]}\"\r\n");
            }

            if (contents.Length > 0)
            {
                File.WriteAllText(Path.Combine(exportFolder, filename), contents.ToString());
            }

            Console.WriteLine("Done!");
            Console.WriteLine("Export Completed! Press Enter to exit.");
            Console.ReadLine();
        }

        static string SanitizeFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
