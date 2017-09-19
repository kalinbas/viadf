using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using viadflib;

namespace PoiExporter
{


    class Program
    {
        static void Main(string[] args)
        {
            Import();
            //CreateScripts();
            //ExtractInfoFromFiles();
            Console.ReadKey();
        }

        static void Import()
        {
            List<string> lines = new List<string>();

            var csv = new CsvReader(new StreamReader(File.OpenRead(@"C:\denue_inegi_09_.csv")));
            csv.Configuration.CultureInfo = CultureInfo.InvariantCulture;
            var i = 0;
            using (var dc = new DataContext())
            {
                while (csv.Read())
                {
                    if (0 <= i)
                    {
                        var b = new Business();
                        b.Category = csv.GetField<string>(4).Replace("'", "''");
                        b.Email = Regex.Replace(csv.GetField<string>(35), "<.*?>", String.Empty).Replace("'", "''");
                        b.Lat = csv.GetField<double>(38);
                        b.Lng = csv.GetField<double>(39);
                        b.Name = csv.GetField<string>(1).Replace("'", "''");
                        b.SeoName = Utils.FormatSEO(b.Name);
                        b.Tel = Regex.Replace(csv.GetField<string>(34), "<.*?>", String.Empty).Replace("'", "''");
                        b.Web = Regex.Replace(csv.GetField<string>(36), "<.*?>", String.Empty).Replace("'", "''");

                        //dc.Businesses.InsertOnSubmit(b);

                        lines.Add(String.Format("INSERT INTO [dbo].[Business] ([Name],[SeoName],[Lat],[Lng],[Tel],[Email],[Web],[Category]) VALUES('{0}','{1}',{2},{3},{4},{5},{6},'{7}')", b.Name, b.SeoName, b.Lat.ToString(CultureInfo.InvariantCulture), b.Lng.ToString(CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(b.Tel) ? "NULL" : "'" + b.Tel + "'", string.IsNullOrWhiteSpace(b.Email) ? "NULL" : "'" + b.Email + "'", string.IsNullOrWhiteSpace(b.Web) ? "NULL" : "'" + b.Web + "'", b.Category));

                        if (i % 100 == 0)
                        {
                            //dc.SubmitChanges();
                        }
                    }
                    i++;
                }
            }

            File.WriteAllLines("lines.sql", lines);

        }

    }
}
