/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DebuggerTests
 * FILE:        DebuggerTests/DebugLogCornerTests.cs
 * PURPOSE:     Tests the Debugger, mostly config file
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        [DoNotParallelize]
        public async Task TestCriticalErrorLogging()
        {
            var initialDebugPath = DebugRegister.DebugPath;
            var initialIsDumpActive = DebugRegister.IsDumpActive;

            var uniqueLogFilePath = Path.Combine(LogDirectory, nameof(TestCriticalErrorLogging) + ".log");

            try
            {
                DebugRegister.DebugPath = nameof(TestCriticalErrorLogging);
                DebugRegister.IsDumpActive = false;

                _debugLog = new DebugLog();
                _debugLog.Start();

                var criticalErrorMessage = "Critical error occurred.";
                var criticalErrorLevel = ErCode.Error;

                // Act
                await Task.Run(() => _debugLog.LogFile(criticalErrorMessage, criticalErrorLevel));

                // Assert
                Assert.IsTrue(await WaitForFileCreationAsync(uniqueLogFilePath), "Log file was not created for critical error.");
            }
            finally
            {
                DebugRegister.DebugPath = initialDebugPath;
                DebugRegister.IsDumpActive = initialIsDumpActive;

                if (File.Exists(uniqueLogFilePath))
                {
                    File.Delete(uniqueLogFilePath);
                }
            }
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task TestNonCriticalErrorLoggingInVerboseMode()
        {
            var initialDebugPath = DebugRegister.DebugPath;
            var initialIsVerbose = DebugRegister.IsVerbose;

            var uniqueLogFilePath = Path.Combine(LogDirectory, nameof(TestNonCriticalErrorLoggingInVerboseMode) + ".log");

            try
            {
                DebugRegister.DebugPath = nameof(TestNonCriticalErrorLoggingInVerboseMode);
                DebugRegister.IsVerbose = true;

                _debugLog = new DebugLog();
                _debugLog.Start();

                var nonCriticalErrorMessage = "Non-critical error occurred.";
                var nonCriticalErrorLevel = ErCode.Warning;

                // Act
                await Task.Run(() => _debugLog.LogFile(nonCriticalErrorMessage, nonCriticalErrorLevel));

                // Assert
                Assert.IsTrue(await WaitForFileCreationAsync(uniqueLogFilePath), "Log file was not created for non-critical error in verbose mode.");
                var content = await File.ReadAllTextAsync(uniqueLogFilePath);
                Assert.IsTrue(content.Contains(nonCriticalErrorMessage), "Non-critical error message was not logged in verbose mode.");
            }
            finally
            {
                DebugRegister.DebugPath = initialDebugPath;
                DebugRegister.IsVerbose = initialIsVerbose;

                if (File.Exists(uniqueLogFilePath))
                {
                    File.Delete(uniqueLogFilePath);
                }
            }
        }


        [TestMethod]
        [DoNotParallelize]
        public async Task TestLogFileBehaviorWithoutVerboseMode()
        {
            var initialDebugPath = DebugRegister.DebugPath;
            var initialIsDumpActive = DebugRegister.IsDumpActive;

            var uniqueLogFilePath = Path.Combine(LogDirectory, nameof(TestLogFileBehaviorWithoutVerboseMode) + ".log");

            try
            {
                DebugRegister.DebugPath = nameof(TestLogFileBehaviorWithoutVerboseMode);
                DebugRegister.IsDumpActive = false;

                _debugLog = new DebugLog();
                _debugLog.Start();

                var criticalErrorMessage = "Critical error occurred.";
                var criticalErrorLevel = ErCode.Error;

                // Act
                await Task.Run(() => _debugLog.LogFile(criticalErrorMessage, criticalErrorLevel));

                // Assert
                Assert.IsTrue(await WaitForFileCreationAsync(uniqueLogFilePath), "Log file was not created without verbose mode.");
            }
            finally
            {
                DebugRegister.DebugPath = initialDebugPath;
                DebugRegister.IsDumpActive = initialIsDumpActive;

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
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(5);
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