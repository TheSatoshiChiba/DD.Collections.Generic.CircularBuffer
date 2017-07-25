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
    public sealed class CircularBuffer<TValue> : IReadOnlyCollection<TValue>, ICollection {
        private static readonly int DEFAULT_CAPACITY = 1023;

        public int Count => ( buffer.Length + head - tail ) % buffer.Length;

        public bool IsSynchronized => false;

        public object SyncRoot => syncRoot;

        private readonly object syncRoot = new object();

        private readonly TValue[] buffer;

        private int head;
        private int tail;
        private int version;

        public CircularBuffer()
            : this( DEFAULT_CAPACITY ) {
        }

        public CircularBuffer( int capacity )
            => buffer = new TValue[ capacity + 1 ];

        public void Push( TValue item ) {
            buffer[ head ] = item;

            head = ( buffer.Length + head + 1 ) % buffer.Length;
            if ( head == tail ) {
                tail = ( buffer.Length + tail + 1 ) % buffer.Length;
            }
            version += 1;
        }

        public TValue Pop() {
            if ( tail == head ) {
                throw new InvalidOperationException( "Buffer is empty." );
            }

            int index = tail;
            tail = ( buffer.Length + tail + 1 ) % buffer.Length;

            version += 1;
            return buffer[ index ];
        }

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

        public IEnumerator<TValue> GetEnumerator() => new Enumerator( this );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection.CopyTo( Array array, int index )
            => CopyTo( ( TValue[] )array, index );

        private struct Enumerator : IEnumerator<TValue> {
            public TValue Current {
                get {
                    if ( index < 0 ) {
                        if ( index < -1 ) {
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

            public Enumerator( CircularBuffer<TValue> instance ) {
                this.instance = instance;

                index = -1;
                current = default( TValue );
                version = instance.version;
            }

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

            public void Reset() {
                if ( version != instance.version ) {
                    throw new InvalidOperationException(
                        "Buffer changed during enumeration." );
                }

                index = -1;
                current = default( TValue );
            }

            public void Dispose() {
                index = -2;
                current = default( TValue );
            }

            object IEnumerator.Current => Current;
        }
    }
}
