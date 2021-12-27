﻿using System.Collections.Generic;

namespace Blackboard.Core.Data.Interfaces {

    /// <summary>This indicates that this Blackboard data type can be bitwise modified.</summary>
    /// <typeparam name="T">The type of the data implementing this interface.</typeparam>
    public interface IBitwise<T>: IData
        where T : IData {

        /// <summary>This gets the bitwise NOT of this data value.</summary>
        /// <returns>The bitwise NOT of this value.</returns>
        T BitwiseNot();

        /// <summary>This gets the bitwise AND of the other values.</summary>
        /// <remarks>This does not use the current value in the AND.</remarks>
        /// <param name="other">The other values to bitwise AND with.</param>
        /// <returns>The result of the bitwise AND.</returns>
        T BitwiseAnd(IEnumerable<T> other);

        /// <summary>This gets the bitwise OR of the other values.</summary>
        /// <remarks>This does not use the current value in the OR.</remarks>
        /// <param name="other">The other values to bitwise OR with.</param>
        /// <returns>The result of the bitwise OR.</returns>
        T BitwiseOr(IEnumerable<T> other);

        /// <summary>This gets the bitwise XOR of the other values.</summary>
        /// <remarks>This does not use the current value in the XOR.</remarks>
        /// <param name="other">The other values to bitwise XOR with.</param>
        /// <returns>The result of the bitwise XOR.</returns>
        T BitwiseXor(IEnumerable<T> other);

        /// <summary>This gets the left shift of this value by the other value.</summary>
        /// <param name="other">The number of bits to shift by.</param>
        /// <returns>The left shifted value.</returns>
        T LeftShift(T other);

        /// <summary>This gets the right shift of this value by the other value.</summary>
        /// <param name="other">The number of bits to shift by.</param>
        /// <returns>The right shifted value.</returns>
        T RightShift(T other);
    }
}
