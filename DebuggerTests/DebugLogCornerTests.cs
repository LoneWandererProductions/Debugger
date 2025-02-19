/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     DebuggerTests
 * FILE:        DebuggerTests/DebugLogCornerTests.cs
 * PURPOSE:     Tests the Debugger, mostly config file
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebuggerTests
{
    /// <summary>
    /// Tests related to logging behavior of DebugLog.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DebugLogCornerTests
    {
        /// <summary>
        /// The log directory
        /// </summary>
        private static readonly string LogDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebuggerResources.LogPath);

        /// <summary>
        /// The debug log
        /// </summary>
        private DebugLog _debugLog;

        /// <summary>
        /// Tests the critical error logging.
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public async Task TestCriticalErrorLogging()
        {
            var logFile = PrepareTest(nameof(TestCriticalErrorLogging));

            try
            {
                InitializeDebugLog();
                _debugLog.LogFile("Critical error occurred.", ErCode.Error);

                Assert.IsTrue(await WaitForFileCreationAsync(logFile), "Log file was not created for critical error.");
            }
            finally
            {
                CleanupDebugSettings(logFile);
            }
        }

        /// <summary>
        /// Tests the non critical error logging in verbose mode.
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public async Task TestNonCriticalErrorLoggingInVerboseMode()
        {
            var logFile = PrepareTest(nameof(TestNonCriticalErrorLoggingInVerboseMode));

            try
            {
                DebugRegister.IsVerbose = true;
                InitializeDebugLog();
                _debugLog.LogFile("Non-critical error occurred.", ErCode.Warning);

                Assert.IsTrue(await WaitForFileCreationAsync(logFile), "Log file was not created for non-critical error.");
                Assert.IsTrue(await FileContainsAsync(logFile, "Non-critical error occurred."),
                              "Log file does not contain expected message.");
            }
            finally
            {
                CleanupDebugSettings(logFile);
            }
        }

        /// <summary>
        /// Tests the log file behavior without verbose mode.
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public async Task TestLogFileBehaviorWithoutVerboseMode()
        {
            var logFile = PrepareTest(nameof(TestLogFileBehaviorWithoutVerboseMode));

            try
            {
                InitializeDebugLog();
                _debugLog.LogFile("Critical error occurred.", ErCode.Error);

                Assert.IsTrue(await WaitForFileCreationAsync(logFile), "Log file was not created without verbose mode.");
            }
            finally
            {
                CleanupDebugSettings(logFile);
            }
        }

        /// <summary>
        /// Prepares the test.
        /// </summary>
        /// <param name="testName">Name of the test.</param>
        /// <returns></returns>
        private static string PrepareTest(string testName)
        {
            DebugRegister.DebugName = testName + ".log";
            return Path.Combine(LogDirectory, DebugRegister.DebugName);
        }

        private void InitializeDebugLog()
        {
            _debugLog = new DebugLog();
            _debugLog.Start();
        }

        /// <summary>
        /// Cleanups the debug settings.
        /// </summary>
        /// <param name="logFile">The log file.</param>
        private static void CleanupDebugSettings(string logFile)
        {
            DebugRegister.DebugName = string.Empty;
            DebugRegister.IsVerbose = false;
            DebugRegister.IsDumpActive = false;

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }

        /// <summary>
        /// Waits for a file to be created asynchronously.
        /// </summary>
        private static async Task<bool> WaitForFileCreationAsync(string filePath, TimeSpan? timeout = null)
        {
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < effectiveTimeout)
            {
                if (File.Exists(filePath))
                {
                    return true;
                }

                await Task.Delay(50);
            }

            return false;
        }

        /// <summary>
        /// Checks if a file contains the given text.
        /// </summary>
        private static async Task<bool> FileContainsAsync(string filePath, string expectedText)
        {
            return File.Exists(filePath) && (await File.ReadAllTextAsync(filePath)).Contains(expectedText);
        }
    }
}
