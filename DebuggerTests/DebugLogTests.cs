/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DebuggerTests
 * FILE:        DebuggerTests/DebugLogTests.cs
 * PURPOSE:     Tests the Debugger, mostly config file
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DebuggerTests
{
    /// <summary>
    /// Class that tests all stuff related to log files
    /// </summary>
    [TestClass]
    public class DebugLogTests
    {
        private const string TestDebugName = "test_debug.log";
        private const string DeleteDebugName = "delete_debug.log";
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebuggerResources.LogPath);
        private DebugLog _debugLog;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _debugLog = new DebugLog();
            DebugRegister.DebugName = TestDebugName;
            _debugLog.Start();
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            DeleteLogFile(TestDebugName);
            DeleteLogFile(DeleteDebugName);
        }

        /// <summary>
        /// Tests the delete log file.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteLogFile()
        {
            DebugRegister.DebugName = DeleteDebugName;
            var errorMessage = "Test Error";
            var errorLevel = ErCode.Error;

            await Task.Run(() => _debugLog.LogFile(errorMessage, errorLevel));
            File.WriteAllText(TestDebugName, "Test Content");
            _debugLog.Delete();

            var target = Path.Combine(LogDirectory, DeleteDebugName);
            Assert.IsFalse(await WaitForFileCreationAsync(target), "Log file was not deleted.");
            DebugRegister.DebugName = TestDebugName;
        }

        /// <summary>
        /// Tests the log file creation.
        /// </summary>
        [TestMethod]
        public async Task TestLogFileCreation()
        {
            var errorMessage = "Test Error";
            var errorLevel = ErCode.Error;

            await Task.Run(() => _debugLog.LogFile(errorMessage, errorLevel));
            var target = Path.Combine(LogDirectory, TestDebugName);

            Assert.IsTrue(await WaitForFileCreationAsync(target), "Log file was not created.");
            var content = File.ReadAllText(target);
            Assert.IsTrue(content.Contains(errorMessage), "Error message was not logged.");
        }

        /// <summary>
        /// Tests the start stop debugging.
        /// </summary>
        [TestMethod]
        public void TestStartStopDebugging()
        {
            DebugRegister.IsRunning = false;
            _debugLog.Start();
            var isRunningAfterStart = DebugRegister.IsRunning;

            _debugLog.StopDebugging();
            var isRunningAfterStop = DebugRegister.IsRunning;

            Assert.IsTrue(isRunningAfterStart, "Debugging did not start.");
            Assert.IsFalse(isRunningAfterStop, "Debugging did not stop.");
        }

        /// <summary>
        /// Tests the initiate window.
        /// </summary>
        [TestMethod]
        public void TestInitiateWindow()
        {
            _debugLog.StartWindow();
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(DebuggerResources.TrailWindow));
            Assert.IsTrue(processes.Length > 0, "Window process was not started.");
            _debugLog.CloseWindow();
        }

        /// <summary>
        /// Waits for file creation asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns></returns>
        private static async Task<bool> WaitForFileCreationAsync(string filePath, TimeSpan? timeout = null)
        {
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);
            var start = DateTime.Now;

            while ((DateTime.Now - start) < effectiveTimeout)
            {
                if (File.Exists(filePath)) return true;
                await Task.Delay(50);
            }

            return false;
        }

        /// <summary>
        /// Deletes the log file.
        /// </summary>
        /// <param name="logName">Name of the log.</param>
        private static void DeleteLogFile(string logName)
        {
            var debugPath = Path.Combine(LogDirectory, $"{logName}{DebuggerResources.LogFileExtension}");
            if (File.Exists(debugPath)) File.Delete(debugPath);
        }
    }
}
