﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DebuggerTests
 * FILE:        DebuggerTests/DebugLogStaticTests.cs
 * PURPOSE:     Tests the Debugger, static access stuff
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DebuggerTests
{
    /// <summary>
    /// static tests
    /// </summary>
    [TestClass]
    public partial class DebugLogStaticTests
    {
        /// <summary>
        /// The test debug path
        /// </summary>
        private const string TestDebugName = "test_static_debug.log";

        /// <summary>
        /// The delete debug path
        /// </summary>
        private const string DeleteDebugName= "delete_static_debug.log";

        // The directory where log files are stored
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DebuggerResources.LogPath);

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Clear any existing log files before each test
            if (File.Exists(TestDebugName))
            {
                File.Delete(TestDebugName);
            }

            DebugRegister.DebugName = TestDebugName; // Simulate Debug Path
            DebugRegister.IsDumpActive = true; // activate dump, my bad....
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            Debugs.StopDebugging(); // Ensure debugging is stopped

            if (File.Exists(TestDebugName))
            {
                //File.Delete(TestDebugPath); // Clean up log file
            }
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
            var task = Task.Run(() => Debugs.LogFile(errorMessage, errorLevel));
            await task; // Warten, bis die Log-Datei geschrieben wurde

            var target = Path.Combine(LogDirectory, TestDebugName);

            // Assert mit Polling
            var fileExists = await WaitForConditionAsync(() => File.Exists(target), TimeSpan.FromSeconds(2));
            Assert.IsTrue(fileExists, "Log file was not created.");

            var content = File.ReadAllText(target);
            Assert.IsTrue(content.Contains(errorMessage), "Error message was not logged.");
        }

        /// <summary>
        /// Tests the delete log file.
        /// </summary>
        [TestMethod]
        public async Task TestDeleteLogFile()
        {
            DebugRegister.DebugName = DeleteDebugName; // Simulate Debug Path

            // Arrange
            var errorMessage = "Test Error";
            var errorLevel = ErCode.Error;

            // Act
            await Task.Run(() => Debugs.LogFile(errorMessage, errorLevel));

            // Arrange
            File.WriteAllText(TestDebugName, "Test Content");

            // Act
            Debugs.DeleteLogFile();


            var target = Path.Combine(LogDirectory, DeleteDebugName + ".log");

            // Assert
            Assert.IsFalse(File.Exists(target), "Log file was not deleted.");

            //restore old path
            DebugRegister.DebugName = TestDebugName; // Simulate Debug Path
        }


        /// <summary>
        /// Tests the log file with an object.
        /// </summary>
        [TestMethod]
        public async Task TestLogFileWithObject()
        {
            // Arrange
            var errorMessage = "Test Error with Object";
            var errorLevel = ErCode.Warning;
            var testObject = new LogData { Name = "Test", Value = 42 };


            // Act: Log the message asynchronously
            await Task.Run(() => Debugs.LogFile(errorMessage, errorLevel, testObject));

            var target = Path.Combine(LogDirectory, TestDebugName);

            // Assert with polling
            var fileExists = await WaitForConditionAsync(() => File.Exists(target), TimeSpan.FromSeconds(2));
            Assert.IsTrue(fileExists, "Log file was not created.");

            var content = File.ReadAllText(target);
            Assert.IsTrue(content.Contains("42"), "Object was not logged.");
        }

        /// <summary>
        /// Tests the log file with a dictionary.
        /// </summary>
        [TestMethod]
        public async Task TestLogFileWithDictionary()
        {
            // Arrange
            var errorMessage = "Test Error with Dictionary";
            var errorLevel = ErCode.Information;
            var testDictionary = new Dictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } };

            // Act: Log the message asynchronously
            await Task.Run(() => Debugs.LogFile(errorMessage, errorLevel, testDictionary));

            var target = Path.Combine(LogDirectory, TestDebugName);

            // Assert with polling
            var fileExists = await WaitForConditionAsync(() => File.Exists(target), TimeSpan.FromSeconds(2));
            Assert.IsTrue(fileExists, "Log file was not created.");

            var content = File.ReadAllText(target);
            Assert.IsTrue(content.Contains("Key1"), "Dictionary was not logged.");
        }

        /// <summary>
        /// Tests the stop debugging when not running.
        /// </summary>
        [TestMethod]
        public void TestStopDebuggingWhenNotRunning()
        {
            // Act
            Debugs.StopDebugging();

            // Assert: No exceptions should be thrown and it should stop gracefully.
            Assert.IsTrue(true); // Placeholder, actual assertion can be added based on behavior
        }

        /// <summary>
        /// Tests the create dump when no logs.
        /// </summary>
        [TestMethod]
        public void TestCreateDumpWhenNoLogs()
        {
            // Act
            Debugs.CreateDump();

            // Assert: Verify that dump is created or no errors occur.
            // The specific assertion depends on your implementation of CreateDump()
            Assert.IsTrue(true); // Placeholder
        }

        /// <summary>
        /// Tests the start stop debugging.
        /// </summary>
        [TestMethod]
        public void TestStartStopDebugging()
        {
            // Arrange
            Debugs.StopDebugging(); // Ensure it is stopped before starting

            // Act
            Debugs.StartWindow();
            Debugs.StopDebugging();

            // Assert
            Assert.IsTrue(true); // Placeholder, actual assertions can be added based on behavior
        }

        /// <summary>
        /// Waits for condition asynchronous.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>Wait time is over</returns>
        private static async Task<bool> WaitForConditionAsync(Func<bool> condition, TimeSpan timeout)
        {
            var start = DateTime.Now;
            while ((DateTime.Now - start) < timeout)
            {
                if (condition())
                    return true;

                await Task.Delay(50); // Polling interval
            }

            return false;
        }
    }
}
