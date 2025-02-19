﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/DebugProcessing.cs
 * PURPOSE:     Handle the messages
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable SwitchStatementMissingSomeCases

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Debugger
{
    /// <summary>
    ///     Static class for handling debug processing and logging.
    /// </summary>
    internal static class DebugProcessing
    {
        /// <summary>
        ///     Controls whether debugging is active.
        /// </summary>
        private static bool _debugLogging;

        /// <summary>
        ///     Semaphore for writing logs to prevent multiple threads from writing simultaneously.
        /// </summary>
        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        /// <summary>
        ///     Handles cancellation of async operations.
        /// </summary>
        private static readonly CancellationTokenSource CancellationTokenSource = new();

        /// <summary>
        ///     Creates a log entry.
        /// </summary>
        internal static void DebugLogEntry(string errorMessage, ErCode logLevel, string stackTrace, string logFile)
        {
            if (!_debugLogging)
            {
                return;
            }

            var logMessage = CreateLogMessage(errorMessage, string.Empty, logLevel, stackTrace);
            HandleLogMessage(logMessage, logLevel, logFile);
        }

        /// <summary>
        ///     Creates a log entry for an object.
        /// </summary>
        internal static void DebugLogEntry<T>(string errorMessage, ErCode logLevel, T obj, string stackTrace,
            string logFile)
        {
            if (!_debugLogging)
            {
                return;
            }

            var objectString = ConvertObjectXml.ConvertObjectToXml(obj);
            var logMessage = CreateLogMessage(errorMessage, objectString, logLevel, stackTrace);
            HandleLogMessage(logMessage, logLevel, logFile);
        }

        /// <summary>
        ///     Creates a log entry for a list of objects.
        /// </summary>
        internal static void DebugLogEntry<T>(string errorMessage, ErCode logLevel, IEnumerable<T> objList,
            string stackTrace, string logFile)
        {
            if (!_debugLogging)
            {
                return;
            }

            var objectString = ConvertObjectXml.ConvertListXml(objList);
            var logMessage = CreateLogMessage(errorMessage, objectString, logLevel, stackTrace);
            HandleLogMessage(logMessage, logLevel, logFile);
        }

        /// <summary>
        ///     Creates a log entry for a dictionary of objects.
        /// </summary>
        internal static void DebugLogEntry<T, TU>(string errorMessage, ErCode logLevel, Dictionary<T, TU> objDictionary,
            string stackTrace, string logFile)
        {
            if (!_debugLogging)
            {
                return;
            }

            var objectString = ConvertObjectXml.ConvertDictionaryXml(objDictionary);
            var logMessage = CreateLogMessage(errorMessage, objectString, logLevel, stackTrace);
            HandleLogMessage(logMessage, logLevel, logFile);
        }

        /// <summary>
        ///     Flushes debug logs and activates dump mode.
        /// </summary>
        internal static void DebugFlushActivateDump()
        {
            Trace.Flush();
            DebugRegister.IsDumpActive = true;
        }

        /// <summary>
        ///     Creates a log message.
        /// </summary>
        private static string CreateLogMessage(string errorMessage, string objString, ErCode logLevel,
            string stackTrace)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var logPrefix = logLevel switch
            {
                ErCode.Diagnostic => DebuggerResources.LogLvlVerbose,
                ErCode.Error => DebuggerResources.LogLvlError,
                ErCode.Warning => DebuggerResources.LogLvlWarning,
                ErCode.Information => DebuggerResources.LogLvlInformation,
                ErCode.External => DebuggerResources.LogLvlExternal,
                _ => DebuggerResources.LogLvlInformation
            };

            var logBuilder = new StringBuilder();
            _ = logBuilder.Append(logPrefix)
                .Append(DateTime.Now)
                .Append(DebuggerResources.Spacer)
                .Append(errorMessage);

            if (DebugRegister.IsVerbose)
            {
                _ = logBuilder.Append($"{DebuggerResources.ThreadId}{threadId}")
                    .Append(DebuggerResources.Spacer);
            }

            if (!string.IsNullOrEmpty(objString))
            {
                _ = logBuilder.Append(DebuggerResources.ObjectFormatting).Append(objString);
            }

            if (!string.IsNullOrEmpty(stackTrace))
            {
                _ = logBuilder.Append(DebuggerResources.Spacer)
                    .Append(stackTrace);
            }

            return logBuilder.ToString();
        }

        /// <summary>
        ///     Handles a log message by adding it to the current log and optionally writing it to a file.
        /// </summary>
        private static void HandleLogMessage(string logMessage, ErCode logLevel, string logFile)
        {
            if (DebugLog.Container.Capacity < DebugLog.CurrentLog.Count)
            {
                DebugLog.Container.Clear();
            }

            DebugLog.Container.Add(logMessage);
            Trace.WriteLine(logMessage);

            if (logLevel == ErCode.Error || DebugRegister.IsDumpActive || DebugRegister.IsVerbose)
            {
                _ = WriteLogFileAsync(logFile, logMessage);
            }
        }

        /// <summary>
        ///     Asynchronously writes a log message to a file.
        /// </summary>
        private static async Task WriteLogFileAsync(string logFile, string logMessage)
        {
            await Semaphore.WaitAsync();

            try
            {
                await File.AppendAllTextAsync(logFile, $"{logMessage}{Environment.NewLine}");

                //File.WriteAllText(logFile, $"{logMessage}{Environment.NewLine}");
            }
            finally
            {
                Semaphore.Release();
            }
        }

        /// <summary>
        ///     Starts the debug logging process.
        /// </summary>
        internal static void StartDebug()
        {
            DebugLog.Container = new List<string>();
            DebugRegister.IsRunning = _debugLogging = true;
        }

        /// <summary>
        ///     Stops debugging and closes any active tasks.
        /// </summary>
        internal static void StopDebuggingClose()
        {
            DebugRegister.IsRunning = _debugLogging = false;
            CancellationTokenSource.Cancel();

            Trace.Close();
        }
    }
}
