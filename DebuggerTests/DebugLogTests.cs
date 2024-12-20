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
        /// <summary>
        /// The test debug path
        /// </summary>
        private const string TestDebugPath = "test_debug";


        /// <summary>
        /// The delete debug path
        /// </summary>
        private const string DeleteDebugPath = "delete_debug";

        /// <summary>
        ///     The directory where log files are stored.
        /// </summary>
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebuggerResources.LogPath);

        /// <summary>
        /// The debug log
        /// </summary>
        private DebugLog _debugLog;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _debugLog = new DebugLog();
            DebugRegister.DebugPath = TestDebugPath; // Simulate Debug Path
            _debugLog.Start();
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(TestDebugPath))
            {
                //File.Delete(TestDebugPath);
            }
        }

        /// <summary>
        /// Tests the delete log file.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteLogFile()
        {
            DebugRegister.DebugPath = DeleteDebugPath; // Simulate Debug Path

            // Arrange
            var errorMessage = "Test Error";
            var errorLevel = ErCode.Error;

            // Act
            await Task.Run(() => _debugLog.LogFile(errorMessage, errorLevel));
            
            // Arrange
            File.WriteAllText(TestDebugPath, "Test Content");

            // Act
            _debugLog.Delete();

            var target = Path.Combine(LogDirectory, DeleteDebugPath + ".log");

            // Assert
            Assert.IsFalse(File.Exists(target), "Log file was not deleted.");

            //restore old path
            DebugRegister.DebugPath = TestDebugPath; // Simulate Debug Path
        }

        /// <summary>
        /// Tests the log file creation.
        /// </summary>
        [TestMethod]
        public async Task TestLogFileCreation()
        {
            // Arrange
            var errorMessage = "Test Error";
            var errorLevel = ErCode.Error;

            // Act
            await Task.Run(() => _debugLog.LogFile(errorMessage, errorLevel));

            var target = Path.Combine(LogDirectory, TestDebugPath + ".log");
            // Assert
            Assert.IsTrue(File.Exists(target), "Log file was not created.");
            var content = File.ReadAllText(target);
            Assert.IsTrue(content.Contains(errorMessage), "Error message was not logged.");
        }

        /// <summary>
        /// Tests the start stop debugging.
        /// </summary>
        [TestMethod]
        public void TestStartStopDebugging()
        {
            // Arrange
            DebugRegister.IsRunning = false;

            // Act
            _debugLog.Start();
            var isRunningAfterStart = DebugRegister.IsRunning;

            _debugLog.StopDebugging();
            var isRunningAfterStop = DebugRegister.IsRunning;

            // Assert
            Assert.IsTrue(isRunningAfterStart, "Debugging did not start.");
            Assert.IsFalse(isRunningAfterStop, "Debugging did not stop.");
        }

        /// <summary>
        /// Tests the initiate window.
        /// </summary>
        [TestMethod]
        public void TestInitiateWindow()
        {
            // Act
            _debugLog.StartWindow();

            // Assert
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(DebuggerResources.TrailWindow));
            Assert.IsTrue(processes.Length > 0, "Window process was not started.");

            // Cleanup
            _debugLog.CloseWindow();
        }

        /// <summary>
        /// Tests the close window.
        /// </summary>
        [TestMethod]
        public void TestCloseWindow()
        {
            // Arrange
            _debugLog.StartWindow();

            // Assert
            var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(DebuggerResources.TrailWindow));
            Assert.AreEqual(1, processes.Length, "Window process was not started.");

            // Act
            _debugLog.CloseWindow();

            // Assert
            processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(DebuggerResources.TrailWindow));
            Assert.AreEqual(0, processes.Length, "Window process was not closed.");
        }
    }
}
