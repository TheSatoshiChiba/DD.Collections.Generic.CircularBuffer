// The MIT License(MIT)
// 
// Copyright(c) 2017 Daniel Drywa
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace DD.Collections.Generic {
    /// <summary>
    /// Represents a fixed-size, first-in, first-out collection of objects,
    /// that overwrites old values when the capacity is reached.
    /// </summary>
    /// <remarks>
    /// See: https://en.wikipedia.org/wiki/Circular_buffer
    /// </remarks>
    /// <typeparam name="TValue">The type for items in this collection.</typeparam>
    public sealed class CircularBuffer<TValue> : IReadOnlyCollection<TValue>, ICollection {
        private static readonly int DEFAULT_CAPACITY = 1023;

        /// <summary>
        /// Returns the maximum number of elements
        /// that can be stored in the buffer.
        /// </summary>
        public int Capacity => buffer.Length - 1;

        /// <summary>
        /// Returns the number of elements contained in the buffer.
        /// </summary>
        public int Count => ( buffer.Length + head - tail ) % buffer.Length;

        /// <summary>
        /// Returns a value indicating whether access to 
        /// the buffer is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
        /// Returns an object that can be used to synchronize 
        /// access to the buffer.
        /// </summary>
        public object SyncRoot => syncRoot;

        private readonly object syncRoot = new object();

        private readonly TValue[] buffer;

        private int head;
        private int tail;
        private int version;

        /// <summary>
        /// Initialises a new instance of the <see cref="CircularBuffer{TValue}"/> 
        /// class that is empty and has the default capacity.
        /// </summary>
        public CircularBuffer()
            : this( DEFAULT_CAPACITY ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{TValue}"/>
        /// class that is empty and has the specified capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the buffer can contain.</param>
        public CircularBuffer( int capacity )
            => buffer = new TValue[ capacity + 1 ];

        /// <summary>
        /// Adds an object to the end of the buffer.
        /// </summary>
        /// <remarks>
        /// If <see cref="Count"/> already equals the <see cref="Capacity"/>, 
        /// the object at the beginning of the buffer is overwritten, 
        /// and the following element will be the new starting point of the buffer.
        /// 
        /// This method is an O(1) operation.
        /// </remarks>
        /// <param name="item">
        /// The object to add to the buffer. 
        /// The value can be null for reference types.
        /// </param>
        public void Push( TValue item ) {
            buffer[ head ] = item;

            head = ( buffer.Length + head + 1 ) % buffer.Length;
            if ( head == tail ) {
                tail = ( buffer.Length + tail + 1 ) % buffer.Length;
            }
            version += 1;
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the buffer.
        /// </summary>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        /// <returns>The object that is removed from the beginning of the buffer.</returns>
        public TValue Pop() {
            if ( tail == head ) {
                throw new InvalidOperationException( "Buffer is empty." );
            }

            int index = tail;
            tail = ( buffer.Length + tail + 1 ) % buffer.Length;

            version += 1;
            return buffer[ index ];
        }

        /// <summary>
        /// Copies the buffer elements to an existing one-dimensional Array, 
        /// starting at the specified array offset.
        /// </summary>
        /// <param name="destination">
        /// The one-dimensional Array that is the destination of the 
        /// elements copied from the buffer. The Array must have 
        /// zero-based indexing.
        /// </param>
        /// <param name="offset">The zero-based index in array at which copying begins.</param>
        public void CopyTo( TValue[] destination, int offset ) {
            if ( destination == null ) {
                throw new ArgumentNullException( nameof( destination ) );
            }
            if ( offset < 0 ) {
                throw new ArgumentOutOfRangeException(
                    nameof( offset ),
                    "Offset must be greater or equal to 0." );
            }

            int length = Count;
            if ( offset + length > destination.Length
                || offset + length < 0 ) {

                throw new ArgumentException(
                    "No room in the destination array.",
                    nameof( destination ) );
            }

            if ( head == tail ) {
                return;
            }

            if ( head > tail ) {
                Array.Copy( buffer, tail, destination, offset, length );
            } else {
                length = buffer.Length - tail;
                Array.Copy( buffer, tail, destination, offset, length );
                Array.Copy( buffer, 0, destination, offset + length, head );
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the buffer.
        /// </summary>
        /// <returns>The enumerator that can be used to iterate through the buffer.</returns>
        public IEnumerator<TValue> GetEnumerator() => new Enumerator( this );

        /// <summary>
        /// Returns an enumerator that iterates through the buffer.
        /// </summary>
        /// <returns>The enumerator that can be used to iterate through the buffer.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Copies the buffer elements to an existing one-dimensional Array, 
        /// starting at the specified array offset.
        /// </summary>
        /// <param name="destination">
        /// The one-dimensional Array that is the destination of the 
        /// elements copied from the buffer. The Array must have 
        /// zero-based indexing.
        /// </param>
        /// <param name="offset">The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo( Array array, int index )
            => CopyTo( ( TValue[] )array, index );

        /// <summary>
        /// The CircularBuffer enumerator that iterates through the buffer.
        /// </summary>
        private struct Enumerator : IEnumerator<TValue> {
            /// <summary>
            /// Returns the element in the buffer at the current position of the enumerator.
            /// </summary>
            public TValue Current {
                get {
                    if ( index < 0 ) {
                        if ( index == -1 ) {
                            throw new InvalidOperationException(
                                "Enumeration not started." );
                        }

                        throw new InvalidOperationException(
                            "Enumeration finished." );
                    }
                    return current;
                }
            }

            private readonly CircularBuffer<TValue> instance;
            private readonly int version;

            private int index;
            private TValue current;

            /// <summary>
            /// Initialises a new instance of the <see cref="Enumerator"/> struct.
            /// </summary>
            /// <param name="instance">The buffer to enumerate over.</param>
            public Enumerator( CircularBuffer<TValue> instance ) {
                this.instance = instance;

                index = -1;
                current = default( TValue );
                version = instance.version;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the buffer.
            /// </summary>
            /// <returns>
            /// True if the enumerator was successfully advanced to the next element.
            /// False if the enumerator has passed the end of the buffer.
            /// </returns>
            public bool MoveNext() {
                if ( version != instance.version ) {
                    throw new InvalidOperationException( 
                        "Buffer changed during enumeration." );
                }

                if ( index == -2 ) {
                    return false;
                }

                index = index == -1
                    ? instance.tail
                    : ( instance.buffer.Length + index + 1 ) % instance.buffer.Length;

                if ( index == instance.head ) {
                    index = -2;
                    current = default( TValue );
                    return false;
                }

                current = instance.buffer[ index ];
                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, 
            /// which is before the first element in the buffer.
            /// </summary>
            public void Reset() {
                if ( version != instance.version ) {
                    throw new InvalidOperationException(
                        "Buffer changed during enumeration." );
                }

                index = -1;
                current = default( TValue );
            }

            /// <summary>
            /// Disposes the enumerator.
            /// </summary>
            public void Dispose() {
                index = -2;
                current = default( TValue );
            }

            /// <summary>
            /// Returns the element in the buffer at the current position of the enumerator.
            /// </summary>
            object IEnumerator.Current => Current;
        }
    }
}
