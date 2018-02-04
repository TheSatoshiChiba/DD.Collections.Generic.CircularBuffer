// The MIT License(MIT)
//
// Copyright(c) 2017, 2018 Daniel Drywa
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
        bool ICollection.IsSynchronized => false;

        /// <summary>
        /// Returns an object that can be used to synchronize
        /// access to the buffer.
        /// </summary>
        object ICollection.SyncRoot => syncRoot;

        private readonly Lazy<object> syncRoot
            = new Lazy<object>( () => new object() );

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
            // no-op.
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
                buffer[ head ] = default;
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
        /// <exception cref="InvalidOperationException">The buffer is empty.</exception>
        public TValue Pop() {
            if ( tail == head ) {
                throw new InvalidOperationException( "Buffer is empty." );
            }

            var result = buffer[ tail ];

            buffer[ tail ] = default;
            tail = ( buffer.Length + tail + 1 ) % buffer.Length;

            version += 1;
            return result;
        }

        /// <summary>
        /// Returns the object at the beginning of the buffer without removing it.
        /// </summary>
        /// <remarks>
        /// This method is similar to <see cref="Pop"/> but does not
        /// remove the returned object from the buffer.
        ///
        /// This method is an O(1) operation.
        /// </remarks>
        /// <returns>The object at the beginning of the buffer.</returns>
        /// <exception cref="InvalidOperationException">The buffer is empty.</exception>
        public TValue Peek() {
            if ( tail == head ) {
                throw new InvalidOperationException( "Buffer is empty." );
            }

            return buffer[ tail ];
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="CircularBuffer{TValue}"/>.
        /// </summary>
        /// <param name="value">
        /// The object to locate in the <see cref="CircularBuffer{TValue}"/>.
        /// The value can be null for reference types.
        /// </param>
        /// <returns>
        /// True if item is found in the <see cref="CircularBuffer{TValue}"/>.
        /// Otherwise false.
        /// </returns>
        /// <remarks>
        /// This method determines equality using the default equality comparer
        /// <see cref="EqualityComparer{T}.Default"/> for <see cref="TValue"/>.
        /// 
        /// This method is an O(n) operation, where n is <see cref="Count"/>.
        /// </remarks>
        public bool Contains( TValue value ) {
            if ( tail == head ) {
                return false;
            }

            var index = tail;
            var comparer = EqualityComparer<TValue>.Default;
            while ( index != head ) {
                if ( comparer.Equals( buffer[ index ], value ) ) {
                    return true;
                }
                index = ( buffer.Length + index + 1 ) % buffer.Length;
            }

            return false;
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
        /// <exception cref="ArgumentNullException">The destination array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The given offset is negative or bigger than the actual array..</exception>
        /// <exception cref="ArgumentException">The destination array is too small to hold a copy of all elements within the buffer.</exception>
        public void CopyTo( TValue[] destination, int offset ) {
            if ( destination == null ) {
                throw new ArgumentNullException( nameof( destination ) );
            }
            if ( offset < 0 || offset > destination.Length ) {
                throw new ArgumentOutOfRangeException(
                    nameof( offset ),
                    "Offset is negative or larger than the destination." );
            }

            var length = Count;
            if ( destination.Length - offset < Count ) {
                throw new ArgumentException(
                    "No room in the destination array.",
                    nameof( destination ) );
            }

            CopyBuffer( destination, offset );
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
        /// <exception cref="ArgumentNullException">The destination array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The given offset is negative or bigger than the actual array..</exception>
        /// <exception cref="ArgumentException">
        /// The destination array is eihter too small to hold a copy of all elements within the buffer,
        /// multidimensional, has non-zero-based indexing, or an invalid type.
        /// </exception>
        void ICollection.CopyTo( Array array, int index ) {
            if ( array == null ) {
                throw new ArgumentNullException( nameof( array ) );
            }
            if ( array.Rank != 1 ) {
                throw new ArgumentException( 
                    "Multidimensional Arrays are not supported.", 
                    nameof( array ) );
            }
            if ( array.GetLowerBound( 0 ) != 0 ) {
                throw new ArgumentException( 
                    "Array has non-zero-based indexing.", 
                    nameof( array ) );
            }
            if ( index < 0 || index > array.Length ) {
                throw new ArgumentOutOfRangeException( 
                    nameof( index ), 
                    "Index is negative or larger than the destination." );
            }

            var length = Count;
            if ( array.Length - index < length ) {
                throw new ArgumentException(
                    "No room in the destination array.", 
                    nameof( array ) );
            }

            if ( array is TValue[] values ) {
                CopyTo( values, index );

            } else if ( array is object[] objects ) {
                try {
                    CopyBuffer( objects, index );
                } catch ( ArrayTypeMismatchException ) {
                    throw new ArgumentException(
                        $"Invalid array type. Expected type {typeof( TValue )}.",
                        nameof( array ) );
                }

            } else {
                throw new ArgumentException( 
                    "Invalid array type.", 
                    nameof( array ) );
            }
        }

        /// <summary>
        /// Performs a copy of the internal buffer to the given destination.
        /// </summary>
        /// <remarks>
        /// No error checks are performed in this method. It is up to
        /// the calling method to provide valid arguments.
        /// 
        /// If there are no elements in the buffer then no copy operation will take place.
        /// </remarks>
        /// <param name="destination">The destination array.</param>
        /// <param name="offset">The zero-based index in array at which copying begins.</param>
        private void CopyBuffer( Array destination, int offset ) {
            if ( head == tail ) {
                return;
            }

            if ( head > tail ) {
                Array.Copy( buffer, tail, destination, offset, Count );
            } else {
                var length = buffer.Length - tail;
                Array.Copy( buffer, tail, destination, offset, length );
                Array.Copy( buffer, 0, destination, offset + length, head );
            }
        }

        /// <summary>
        /// The CircularBuffer enumerator that iterates through the buffer.
        /// </summary>
        private struct Enumerator : IEnumerator<TValue> {
            /// <summary>
            /// Returns the element in the buffer at the current position of the enumerator.
            /// </summary>
            /// <exception cref="InvalidOperationException">Enumeration has not started or has already finished.</exception>
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
                current = default;
                version = instance.version;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the buffer.
            /// </summary>
            /// <returns>
            /// True if the enumerator was successfully advanced to the next element.
            /// False if the enumerator has passed the end of the buffer.
            /// </returns>
            /// <exception cref="InvalidOperationException">The buffer changed during enumeration.</exception>
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
                    current = default;
                    return false;
                }

                current = instance.buffer[ index ];
                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position,
            /// which is before the first element in the buffer.
            /// </summary>
            /// <exception cref="InvalidOperationException">The buffer changed during enumeration.</exception>
            public void Reset() {
                if ( version != instance.version ) {
                    throw new InvalidOperationException(
                        "Buffer changed during enumeration." );
                }

                index = -1;
                current = default;
            }

            /// <summary>
            /// Disposes the enumerator.
            /// </summary>
            public void Dispose() {
                index = -2;
                current = default;
            }

            /// <summary>
            /// Returns the element in the buffer at the current position of the enumerator.
            /// </summary>
            object IEnumerator.Current => Current;
        }
    }
}
