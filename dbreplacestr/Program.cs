using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Xml.Linq;

namespace dbreplacestr
{
    internal class Program
    {
        static int versionMajor = 1;
        static int versionMinor = 0;
        static int versionRevision = 0;

        static string logFilePath = "dbreclacestr_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
        static string timestamp = String.Empty;

        static void Usage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Replace text from SQL files.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Usage: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tdbreclacestr");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -i ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("input_file");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -o ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("output_file");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" string1");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" replacement1");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" [ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("string2");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" replacement2");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" ] ...");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tdbreclacestr");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --infile ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("input_file");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --outfile ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("output_file");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" string1");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" replacement1");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" [ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("string2");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" replacement2");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" ] ...");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tdbreclacestr");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" -h");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\tdbreclacestr");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" --help");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Options:");
            Console.WriteLine("\t-i | --infile\tInput filename or file filter.");
            Console.WriteLine("\t-o | --outfile\tOutput file or path, you can use ':ts' to use the same name as the input ");
            Console.WriteLine("\t              \tfile with a timestamp suffixed to the name.");
            Console.WriteLine("\tstring1       \tString to find and replace in infile.");
            Console.WriteLine("\treplacement1  \tString to replace with.");
            Console.WriteLine();
            Console.WriteLine("\t-h | --help\tShow this help message.");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("Third ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("3");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("ye Software Inc. (\u00A9) 2024");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Version: {0}.{1}.{2}. ", versionMajor, versionMinor, versionRevision);
        }

        public static void WriteLog(string logMessage)
        {
            try
            {
                using (StreamWriter writer = System.IO.File.AppendText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}");
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\'WriteLog\' ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("(Error creating or writing to Log file)");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void PrintInfo(string str1, string str2)
        {
            WriteLog("I: " + str1 + " " + str2);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(str1);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(str2);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintWarning(string str1, string str2)
        {
            WriteLog("W: " + str1 + " " + str2);

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(str1);
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(str2);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintReplacement(string str1, string str2)
        {
            WriteLog("I: replace: " + str1 + " with: " + str2);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\t\tReplace: '");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(str1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("' with: '");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(str2);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("'");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintHeader(string status)
        {
            WriteLog("H: " + status);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(status);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintError(string name, string error, string detail)
        {
            WriteLog("E: " + error + ": " + "\'" + name + "\' " + "(" + detail + ")");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(error + ": ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\'" + name + "\' ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("(" + detail + ")");
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintProgress(string file, string outfile)
        {
            WriteLog("P: Processing \'" + file + "\' -> " + outfile + "...");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\t\'" + file + "\' ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("->");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" '" + outfile + "\' ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("...");
        }

        static void PrintIgnoringFile(string file)
        {
            WriteLog("P: Ignoring file:  \'" + file + "\' (No need to edit).");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("\t\'" + file + "\' ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("...");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" [");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("IGNORING");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("]");
        }


        static void PrintProgressResultOk()
        {
            WriteLog("P:     -> [OK]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("OK");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("]");
        }

        static void PrintProgressResultError()
        {
            WriteLog("P:     -> [ERROR]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("]");
        }


        public static (string path, string name) ParseFileName(string fileName)
        {
            string path = Path.GetDirectoryName(fileName);
            string name = Path.GetFileName(fileName);
            
            if (String.Empty == path || 0 == path.Length)
                path = ".\\";
            
            return (path, name);
        }

        static bool IsFilter(string file) {
            return (file.Contains("*") || file.Contains("?"));
        }

        public static bool IsDirectory(string path)
        {
            return Directory.Exists(path);
        }

        public static bool IsFile(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static string GetFileName(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length);
        }

        public static string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName);
        }

        public static void PrintInputInfo(string infile) 
        {
            (string path, string name) = ParseFileName(infile);
            PrintInfo("\tInput directory: ", path);

            if (IsFilter(infile))
            {
                PrintInfo("\tInput filter:", name);
            }
            else
            {
                PrintInfo("\tInput file:", name);
            }
        }

        public static void PrintOutputInfo(String outfile, String infile)
        {
            if (IsDirectory(outfile))
            {
                PrintInfo("\tOutput directory:", outfile);
            }
            if (outfile.Contains(":ts"))
            {
                timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                if (IsFilter(infile))
                    PrintInfo("\tOutput file:", "Same as input using timestamp (" + timestamp + ")");
                else
                    PrintInfo("\tOutput file:", GetFileName(infile) + "_" + timestamp + GetFileExtension(infile));
            }
        }

        //public static string ReplaceWords(string input, string word1, string word2, string replacement)
        //{
        //    string pattern = $@"\b{Regex.Escape(word1)}\s*\r?\n*\s*{Regex.Escape(word2)}\b";
        //    return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
        //}

        static string ReplaceWords(string contents, string w1, string w2, string w3, string replacement)
        {
            string pattern = $@"(?i)\b{Regex.Escape(w1)}\s*{Regex.Escape(w2)}\s*{Regex.Escape(w3)}\b";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.Replace(contents, replacement);
        }

        static string ReplaceWords(string contents, string w1, string w2, string replacement)
        {
            string pattern = $@"(?i)\b{Regex.Escape(w1)}\s*\r?\n*\s*{Regex.Escape(w2)}\b";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.Replace(contents, replacement);
        }

        public static string ReplaceWords(string input, string word, string replacement)
        {
            // string pattern = $@"(?i)\b{Regex.Escape(word)}\b";
            string pattern = $@"\b{Regex.Escape(word)}\b";
            return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
        }


        static string BeforeRegex(string input, Regex regex)
        {
            Match match = regex.Match(input);
            if (match.Success)
            {
                return input.Substring(0, match.Index);
            }
            else
            {
                return input;
            }
        }

        static string SpecialCases(string input)
        {
            if (input.Contains("INF_KPI_UNIDADES_CLIENTE_ANTIGUEDAD"))
            {
                // Special case: "Cuando se especifica SELECT DISTINCT, los elementos de ORDER BY deben aparecer en la lista de selección."
                // input = replaceRegex(input, ",A.FechaFactura", ",FechaFactura"); 
                input = input.Replace(",A.FechaFactura", ",FechaFactura");

            }
            if (input.Contains("VForm38"))
            {
                // Strip everything when EXEC is found (add complements that exist and gives error).
                String find = "EXEC";
                String pattern = $@"(?i)\b{Regex.Escape(find)}\b";
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return BeforeRegex(input, regex);
            }
            //
            if (input.Contains("INF_OPERACIONES_TALLER_REPERCUTIDAS"))
            {
                // Special case: "Cuando se especifica SELECT DISTINCT, los elementos de ORDER BY deben aparecer en la lista de selección."
                // input = replaceRegex(input, "ORDER BY f.FechaFactura,", "ORDER BY FechaFactura,");
                input = input.Replace("ORDER BY f.FechaFactura,", "ORDER BY FechaFactura,");

            }
            if (input.Contains("TOC_InsTarTOC_Backup160306"))
            {
                // Special case: "Cuando se especifica SELECT DISTINCT, los elementos de ORDER BY deben aparecer en la lista de selección."
                // input = replaceRegex(input, "ORDER BY f.FechaFactura,", "ORDER BY FechaFactura,");
                input = input.Replace("('035', @famprov, @famprov, 1)", "('035', @famprov, @famprov, 1, NULL, 0, NULL)");

            }
            if (input.Contains("TOY_LEAD_BusquedaClientes") || input.Contains("TOY_CRMASI_Clientes"))
            {
                // Special case: "Mensaje 207, nivel 16, estado 1, procedimiento TOY_LEAD_BusquedaClientes, línea 42 [línea de inicio de lote 0] El nombre de columna 'Data' no es válido."
                input = input.Replace("(Data)", "(strval)");
            }

            if (input.Contains("TOC_InsTarTOC"))
            {
                // Special case: "Mensaje 213, nivel 16, estado 1, procedimiento TOC_InsTarTOC, línea 120 [línea de inicio de lote 0] El nombre de columna o los valores especificados no corresponden a la definición de la tabla."
                if (input.Contains("@famprov, @famprov, 1, null, 0, Null"))
                    input = input.Replace("@famprov, @famprov, 1, null, 0, Null", 
                            "@famprov, @famprov, 1, null, 0, Null, 0, null, 0, 0");
                else
                    // This should only be triggeretd by the TOC_InsTarTOCbackup
                    input = input.Replace("'035', @famprov, @famprov, 1",
                            "@famprov, @famprov, 1, null, 0, Null, 0, null, 0, 0");
            }
            if (input.Contains("VP_FM_ResumenFacturacion"))
            {
                // Special case: Name is extracted incorrectly from the DB !!! Super wierd.
                input = input.Replace("VP_FM_ResumenFacturacion", "VP_FMG_ResumenFacturacion");
            }
            if (input.Contains("fn_TOYOTA_DetalleFormulario")) {
                input = input.Replace("SELECT TOP 1 Data FROM dbo.Split(C.ListaItems, CHAR(9)) WHERE Id = B.Valor",
                                      "SELECT TOP 1 strval FROM dbo.Split(C.ListaItems, CHAR(9)) WHERE strval = B.Valor");
            }
            //if (input.Contains("[KPI_CounterParts]") ||
            //    input.Contains("[KPI_LaborHoursSpent_Garantia]") ||
            //    input.Contains("[KPI_LaborHoursSpent_General]") ||
            //    input.Contains("[KPI_LaborHoursSpent_Internos]") ||
            //    input.Contains("[KPI_LaborHours_Garantia]") ||
            //    input.Contains("[KPI_LaborHours_General]") ||
            //    input.Contains("[KPI_LaborHours_Internos]") ||
            //    input.Contains("[KPI_LaborSales_Garantia]") ||
            //    input.Contains("[KPI_LaborSales_General]") ||
            //    input.Contains("[KPI_LaborSales_Internos]") ||
            //    input.Contains("[KPI_PartsSales_Garantia]") ||
            //    input.Contains("[KPI_PartsSales_General]") ||
            //    input.Contains("[KPI_PartsSales_Internos]") ||
            //    input.Contains("[KPI_Units_Serviced]")
            //    )
            //{
            //    // Special case: "(Solo se puede especificar una expresión en la lista de selección cuando la subconsulta no se especifica con EXISTS."
            //    input = input.Replace("SELECT * FROM dbo.Split", "SELECT [Data] FROM dbo.Split");
            //}

            return input;
        }

        public static string ReplaceAllText(string content, List<(string, string)> pairs)
        {
            content = ReplaceWords(content, "CREATE", "PROCEDURE", "ALTER PROCEDURE");
            content = ReplaceWords(content, "CREATE", "PROC", "ALTER PROCEDURE");
            content = ReplaceWords(content, "CREATE", "VIEW", "ALTER VIEW");
            content = ReplaceWords(content, "CREATE", "FUNCTION", "ALTER FUNCTION");

            content = ReplaceWords(content, "GO", "");

            content = ReplaceWords(content, "SET", "ANSI_NULLS", "ON", "");
            content = ReplaceWords(content, "SET", "ANSI_NULLS", "OFF", "");

            content = ReplaceWords(content, "SET", "QUOTED_IDENTIFIER", "OFF", "");
            content = ReplaceWords(content, "SET", "QUOTED_IDENTIFIER", "ON", "");

            content = ReplaceWords(content, "SET", "SET", "SET"); // toy_InsVehtall => 'set set @algo = ...'

            content = SpecialCases(content);

            foreach (var pair in pairs) {
                content = ReplaceWords(content, pair.Item1, pair.Item2);
            }

            return content;
        }

        public static void ProcessFile(string infile, string outfile, List<(string, string)> pairs)
        {
            
            string content = String.Empty;
            string newFilePath = String.Empty;

            if (infile.Contains("fn_TOYOTA_LOPD2010") ||
                infile.Contains("fn_TOYOTA_LOPD2015")) {
                PrintIgnoringFile(infile);
                return;
            }

            try
            {
                content = System.IO.File.ReadAllText(infile);
            }
            catch (Exception ex)
            {
                PrintError(infile, "Error reading file", ex.Message);
                return;
            }
            (string path, string name) = ParseFileName(infile);
            content = ReplaceAllText(content, pairs); 

            if (outfile.Contains(":ts"))
            {
                newFilePath = GetFileName(infile) + "_" + timestamp + GetFileExtension(infile);
            }
            else
            {
                newFilePath = outfile;
            }
            PrintProgress(infile, newFilePath);
            try
            {
                System.IO.File.AppendAllText(newFilePath, content);
                PrintProgressResultOk();
            }
            catch
            {
                PrintProgressResultError();
                return;
            }
        }

        public static void process_files(string infile, string outfile, List<(string, string)> pairs)
        {
            (string path, string name) = ParseFileName(infile);
            if (IsFilter(infile))
            {
                string[] fileEntries = Directory.GetFiles(path, name);
                foreach (string fileName in fileEntries) { 
                   ProcessFile(fileName, outfile, pairs);
                }
            }
            else
            {
                ProcessFile(infile, outfile, pairs);
            }
        }

        static void Main(string[] args)
        {
            string infile = String.Empty;
            string outfile = String.Empty;
            bool error = false;

            List<(string, string)> pairs = new List<(string, string)>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i":
                    case "--infile":
                        if (i + 1 < args.Length)
                            infile = args[++i];
                        else
                            PrintError("Fatal Error", "Missing value for -i | --infile option.", "Please read usage.");
                        break;

                    case "-o":
                    case "--outfile":
                        if (i + 1 < args.Length)
                            outfile = args[++i];
                        else
                            PrintError("Fatal Error", "Missing value for -o | --outfile option.", "Please read usage.");
                        break;

                    default:
                        if (i + 1 < args.Length)
                            pairs.Add((args[i], args[++i]));
                        else {
                            error = true;
                            PrintError("Fatal Error", "Missing pair values for string AND its replacement.", "Missing argument");
                        }
                        break;

                }
            }

            if (String.Empty == infile || String.Empty == outfile || 0 == pairs.Count || true == error)
            {
                if (String.Empty == infile) PrintError("Terminating program", "Fatal Error", "No input file or filter given");
                if (String.Empty == outfile) PrintError("Terminating program", "Fatal Error", "No outfile file or filter given");
                if (0 == pairs.Count) PrintError("Terminating program", "Fatal Error", "No pairs of substitutions given");
            }
            else
            {
                PrintHeader("Start of process: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                PrintInputInfo(infile);
                PrintOutputInfo(outfile, infile);
                foreach (var pair in pairs)
                {
                    PrintReplacement(pair.Item1, pair.Item2);
                }

                PrintHeader("    Processing files: ");
                process_files(infile, outfile, pairs);
                PrintHeader("End of process: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
            }
        }
    }
}
