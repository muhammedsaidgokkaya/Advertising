using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Helper
{
    public class PythonRun
    {
        public object RunPythonScript(string scriptPath, params string[] args)
        {
            try
            {
                string arguments = $"\"{scriptPath}\" " + string.Join(" ", args);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        return new { error };
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                return new { error = ex.Message };
            }
        }
    }
}
