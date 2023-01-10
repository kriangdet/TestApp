using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ConsoleAppHelp;
class Program
{
    static void Main(string[] args)
    {
        //var xx = Translate("เกรียงเดช เชื้อบุญจันทร์");
        //dILARmh6ud75ScHthSwQzw==


        //var x = Encrypt("OHM0007");
        //var r = Decrypt(x);


        //Transfer date  Database Table Old To Database Table New
        string[] sTable = {
                "MAS_Email"
                ,"MAS_Email_Config"
                ,"MAS_Email_Noti"
            };

        foreach (var item in sTable)
        {
            Transfer_TableOldInTablesNew(item);
        }

        Console.WriteLine();

        Console.Read();

    }

    public static string Translate_ByGoogleAPI(string word)
    {
        var toLanguage = "en";//English
        var fromLanguage = "th";//Thailand
        var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(word)}";
        var webClient = new WebClient
        {
            Encoding = Encoding.UTF8
        };
        var result = webClient.DownloadString(url);
        try
        {
            result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
            return result;
        }
        catch
        {
            return "Error";
        }
    }

    public static string Encrypt(string clearText)
    {
        string EncryptionKey = "abc123";
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
        }
        return clearText;
    }
    public static string Decrypt(string cipherText)
    {
        try
        {
            string EncryptionKey = "abc1234";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }
        catch
        {
            cipherText = "";
        }

        return cipherText;
    }

    public static DataTable Transfer_TableOldInTablesNew(string tablename)
    {
        object response = new object();
        DataTable dt = new DataTable();
        try
        {
            //PTT_Stakeholder
            string conOLD = @"Data Source=.\SQLEXPRESS;Initial Catalog=SKHEngagement-Prod;Integrated Security=True;";
            string conNEW = @"Data Source=43.229.78.117;Initial Catalog=PTT_SH;Integrated Security=False;User ID=ptt-sh;Password=Pa$$w0rd;Min Pool Size=10;Max Pool Size=200";

            //string conNEW = @"Data Source=.\SQLEXPRESS;Initial Catalog=SKHEngagement-Prod;Integrated Security=True;";
            //string conOLD = @"Data Source=43.229.78.117;Initial Catalog=PTT_SH;Integrated Security=False;User ID=ptt-sh;Password=Pa$$w0rd;Min Pool Size=10;Max Pool Size=200";

            #region Excute
            using (var conn = new SqlConnection(conOLD))
            {
                response = conn.Query<object>("SELECT * FROM " + tablename, commandType: CommandType.Text).ToList();
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(response);
            dt = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(json);

            Console.WriteLine(tablename + " : " + dt.Rows.Count);

            if (dt.Rows.Count > 0)
            {
                using (var conn = new SqlConnection(conNEW))
                {
                    response = conn.Execute("TRUNCATE TABLE " + tablename, commandType: CommandType.Text);
                }

                using (SqlConnection con = new SqlConnection(conNEW))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        sqlBulkCopy.DestinationTableName = tablename;
                        con.Open();
                        sqlBulkCopy.WriteToServer(dt);
                        con.Close();
                    }
                }
            }
            #endregion
        }
        catch (Exception ex)
        {

        }
        return dt;
    }
    public static void SaveDataInTables(DataTable dataTable, string tablename)
    {
        if (dataTable.Rows.Count > 0)
        {
            using (SqlConnection con = new SqlConnection("Data Source=43.229.78.117;Initial Catalog=PTT_SH;Integrated Security=False;User ID=ptt-sh;Password=Pa$$w0rd;Min Pool Size=10;Max Pool Size=200"))
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                {
                    sqlBulkCopy.DestinationTableName = tablename;
                    con.Open();
                    sqlBulkCopy.WriteToServer(dataTable);
                    con.Close();
                }
            }
        }
    }
}