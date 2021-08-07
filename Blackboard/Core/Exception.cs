﻿namespace Blackboard.Core {

    /// <summary>The exceptions for the core of blackboard.</summary>
    public class Exception: System.Exception {

        /// <summary>Creates a new exception.</summary>
        /// <param name="message">The message for this exception.</param>
        public Exception(string message) : base(message) { }

        /// <summary>Creates a new exception.</summary>
        /// <param name="message">The message for this exception.</param>
        /// <param name="inner">The inner exception to this exception.</param>
        public Exception(string message, System.Exception inner) : base(message, inner) { }

        /// <summary>Adds additional key value pair of data to this exception.</summary>
        /// <param name="key">The key for the additional data.</param>
        /// <param name="value">The value for the additional data.</param>
        /// <returns>This exception so that these calls can be chained.</returns>
        public Exception With(string key, object value) {
            string strVal = value?.ToString() ?? "null";
            // TODO: Figure out a better way to make this show up in unit-tests.
            //this.Data.Add(key, strVal);
            //return this;
            return new Exception(this.Message + " [" + key + ": " + strVal + "]");
        }

        #region Predefined...

        /// <summary>An exception for when popping off the parser stack and the item was not what was expected.</summary>
        /// <param name="stackInfo">Information for the item which was popped off the stack.</param>
        /// <param name="expected">Information for what was expected.</param>
        /// <returns>The new exception.</returns>
        static public Exception UnexpectedItemOnTheStack(string stackInfo, string expected) =>
            new Exception("Unexpected item type on the stack.").
                With("Found", stackInfo).
                With("Expected", expected);

        /// <summary>An exception for a function which was defined with an unknown type.</summary>
        /// <param name="type">The type which is unknown.</param>
        /// <param name="typeIndex">The index of the type.</param>
        /// <returns>The new exception.</returns>
        static public Exception UnknownFunctionParamType(System.Type type, string typeName) =>
            new Exception("The type used to define a function is not known by the Blackboard type system.").
                With("type", type.FullName).
                With("type name", typeName);

        /// <summary>An exception for a node or group is being renames and it is invalid.</summary>
        /// <param name="name">The name which is invalid.</param>
        /// <returns>The new exception.</returns>
        static public Exception InvalidName(string name) =>
            new Exception("The given name is not a valid identifier.").
                With("name", name);

        /// <summary>An exception for when a loop is detected adding children to a node.</summary>
        /// <returns>The new exception.</returns>
        static public Exception NodeLoopDetected() =>
            new("May not add children: Loop detected.");

        /// <summary>An exception for when a loop is detected adding a group to a scope.</summary>
        /// <returns>The new exception.</returns>
        static public Exception ScopeLoopDetected() =>
            new("May not add to scope: Loop detected.");

        #endregion
    }
}
