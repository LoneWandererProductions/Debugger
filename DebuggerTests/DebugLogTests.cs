/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DebuggerTests
 * FILE:        DebuggerTests/DebugLogCornerTests.cs
 * PURPOSE:     Tests the Debugger, mostly config file
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DebuggerTests
{
    /// <summary>
    /// Class that tests all stuff related to log files
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DebugLogCornerTests
    {
        /// <summary>
        /// The delete debug path
        /// </summary>
        private const string BehaviourDebugPath = "Behaviour_debug";

        /// <summary>
        ///     The directory where log files are stored.
        /// </summary>
        private static readonly string LogDirectory =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebuggerResources.LogPath);

        /// <summary>
        /// The debug log
        /// </summary>
        private DebugLog _debugLog;

        /// <summary>
        /// Tests the log file behavior.
        /// </summary>
        [TestMethod]
        [DoNotParallelize]
        public async Task TestLogFileBehavior()
        {
            var initialDebugPath = DebugRegister.DebugPath;
            var initialIsDumpActive = DebugRegister.IsDumpActive;
            var initialIsVerbose = DebugRegister.IsVerbose;

            var uniqueLogFilePath = Path.Combine(LogDirectory, Guid.NewGuid() + ".log");

            try
            {
                DebugRegister.DebugPath = uniqueLogFilePath;
                DebugRegister.IsDumpActive = false;
                DebugRegister.IsVerbose = false;

                _debugLog = new DebugLog();
                _debugLog.Start();

                // Arrange
                var criticalErrorMessage = "Critical error occurred.";
                var nonCriticalErrorMessage = "Non-critical error occurred.";
                var criticalErrorLevel = ErCode.Error;
                var nonCriticalErrorLevel = ErCode.Warning;

                // Act & Assert: Test critical error
                await Task.Run(() => _debugLog.LogFile(criticalErrorMessage, criticalErrorLevel));
                Assert.IsTrue(await WaitForFileCreationAsync(uniqueLogFilePath), "Log file was not created for critical error.");

                var content = await File.ReadAllTextAsync(uniqueLogFilePath);
                Assert.IsTrue(content.Contains(criticalErrorMessage), "Critical error message was not logged.");

                // Cleanup
                File.Delete(uniqueLogFilePath);

                // Act & Assert: Test non-critical error
                DebugRegister.IsVerbose = true;
                await Task.Run(() => _debugLog.LogFile(nonCriticalErrorMessage, nonCriticalErrorLevel));
                Assert.IsTrue(await WaitForFileCreationAsync(uniqueLogFilePath), "Log file was not created for non-critical error with verbose mode.");

                content = await File.ReadAllTextAsync(uniqueLogFilePath);
                Assert.IsTrue(content.Contains(nonCriticalErrorMessage), "Non-critical error message was not logged in verbose mode.");
            }
            finally
            {
                // Restore original DebugRegister state
                DebugRegister.DebugPath = initialDebugPath;
                DebugRegister.IsDumpActive = initialIsDumpActive;
                DebugRegister.IsVerbose = initialIsVerbose;

                // Cleanup
                if (File.Exists(uniqueLogFilePath))
                {
                    File.Delete(uniqueLogFilePath);
                }
            }
        }


        /// <summary>
        /// Waits for a file to be created asynchronously.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <param name="timeout">The timeout duration.</param>
        /// <returns>True if the file was created; otherwise, false.</returns>
        private static async Task<bool> WaitForFileCreationAsync(string filePath, TimeSpan? timeout = null)
        {
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);
            var start = DateTime.Now;

            while ((DateTime.Now - start) < effectiveTimeout)
            {
                if (File.Exists(filePath))
                {
                    return true;
                }

                await Task.Delay(50); // Polling interval
            }

            return false;
        }
    }
}