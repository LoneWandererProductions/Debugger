/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     DebuggerTests
 * FILE:        DebuggerTests/LogData.cs
 * PURPOSE:     Tests the Debugger, test object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace DebuggerTests
{
    /// <summary>
    /// Test object, we use for testing the object serialization
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int Value { get; set; }
    }
}
