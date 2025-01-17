﻿using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;

namespace TestPerformanceReportGenerator.Utilities
{
    /// <summary>
    /// Helper class which automates test run
    /// </summary>
    public class TestAutoRunner
    {
        private string output;
        private string errorOut;
        public string passedTest;
        public string skippedTest;
        public string failedTest;
        public string duration;
        public string totalTests;

        /// <summary>
        /// Find test run results values. (e.g. Passed Tests, Failed Tests, Skipped Tests, Total number of tests, Duration of tests)
        /// </summary>
        /// <param name="target">Target substring to search for.</param>
        /// <returns>The value of the specified target of a test run</returns>
        private string findSubString(string target)
        {
            int targetIndex = this.output.IndexOf(target);
            int len;
            string substr;
            if (targetIndex != -1)
            {
                if (target.Equals("Duration:"))
                {
                    len = target.Length;
                    substr = this.output.Substring(targetIndex + len + 1, 5);
                    substr = substr.Replace("ms", "");
                    return substr;
                }
                len = target.Length;
                substr = this.output.Substring(targetIndex + len + 5, 1);
                return substr;
            }
            return null;
        }
        /// <summary>
        /// Creates a process that calls the command line to run the command "dotnet test" to retrive test run information.
        /// </summary>
        public void runTestCases()
        {
            // Get the directory that contain the solution such that process can 
            // start in that directory
            DirectoryInfo directory = FileHelper.GetSolutionDir();
            if (directory != null)
            {
                // Set stringBuilders to fetch the output of the process
                StringBuilder outputString = new StringBuilder();
                StringBuilder errorString = new StringBuilder();

                // Start the process and call cmd.exe
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = directory.FullName;
                //Asynchronously process the process output
                process.OutputDataReceived += new DataReceivedEventHandler(
                    delegate (object sender, DataReceivedEventArgs e)
                    {
                        outputString.Append(e.Data);
                    }
                );
                process.ErrorDataReceived += new DataReceivedEventHandler(
                    delegate (object sender, DataReceivedEventArgs e)
                    {
                        errorString.Append(e.Data);
                    }
                );
                // start the process and run the dotnet test command
                process.Start();
                process.StandardInput.WriteLine("dotnet test");
                process.StandardInput.Flush();
                process.StandardInput.Close();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                this.output = outputString.ToString();
                this.errorOut = errorString.ToString();

                // get the number of Passed, Failed, Skipped,
                this.passedTest = findSubString("Passed:");
                this.failedTest = findSubString("Failed:");
                this.skippedTest = findSubString("Skipped:");
                this.duration = findSubString("Duration:");
                this.totalTests = findSubString("Total:");
            }
            else
            {
                //Console.WriteLine("Could not find a directory with the current solution");
                VS.MessageBox.ShowError("Code Quality Report Generator", "Could not find directory containing current solution.");
            }

        }
    }
}