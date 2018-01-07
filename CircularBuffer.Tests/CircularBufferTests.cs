﻿// The MIT License(MIT)
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

using NUnit.Framework;
using System;
using System.Collections;

namespace DD.Collections.Generic.Tests {
    /// <summary>
    /// The <see cref="CircularBuffer{TValue}"/> tests.
    /// </summary>
    public class CircularBufferTests {
        /// <summary>
        /// Tests the constructors.
        /// </summary>
        [Test]
        public void CreationTest() {
            var buffer1 = new CircularBuffer<int>();

            Assert.That( buffer1.Capacity, Is.EqualTo( 1023 ) );
            Assert.That( buffer1.Count, Is.EqualTo( 0 ) );
            Assert.That( buffer1.IsSynchronized, Is.False );
            Assert.That( buffer1.SyncRoot, Is.Not.Null );
            Assert.That( buffer1.SyncRoot, Is.SameAs( buffer1.SyncRoot ) );

            var buffer2 = new CircularBuffer<int>( 42 );

            Assert.That( buffer2.Capacity, Is.EqualTo( 42 ) );
            Assert.That( buffer2.Count, Is.EqualTo( 0 ) );
            Assert.That( buffer2.IsSynchronized, Is.False );
            Assert.That( buffer2.SyncRoot, Is.Not.Null );
            Assert.That( buffer2.SyncRoot, Is.Not.SameAs( buffer1.SyncRoot ) );
        }

        /// <summary>
        /// Various <see cref="CircularBuffer{TValue}.Push(TValue)"/>
        /// and <see cref="CircularBuffer{TValue}.Pop"/> tests.
        /// </summary>
        [Test]
        public void PushPopTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            Assert.That( buffer.Count, Is.EqualTo( 1 ) );

            buffer.Push( 2 );
            Assert.That( buffer.Count, Is.EqualTo( 2 ) );

            buffer.Push( 3 );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );

            Assert.That( buffer.Pop(), Is.EqualTo( 1 ) );
            Assert.That( buffer.Count, Is.EqualTo( 2 ) );
            Assert.That( buffer.Pop(), Is.EqualTo( 2 ) );
            Assert.That( buffer.Count, Is.EqualTo( 1 ) );
            Assert.That( buffer.Pop(), Is.EqualTo( 3 ) );
            Assert.That( buffer.Count, Is.EqualTo( 0 ) );

            buffer.Push( 4 );
            Assert.That( buffer.Count, Is.EqualTo( 1 ) );

            buffer.Push( 5 );
            Assert.That( buffer.Count, Is.EqualTo( 2 ) );

            buffer.Push( 6 );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );

            Assert.That( buffer.Pop(), Is.EqualTo( 4 ) );
            Assert.That( buffer.Count, Is.EqualTo( 2 ) );
            Assert.That( buffer.Pop(), Is.EqualTo( 5 ) );
            Assert.That( buffer.Count, Is.EqualTo( 1 ) );
            Assert.That( buffer.Pop(), Is.EqualTo( 6 ) );
            Assert.That( buffer.Count, Is.EqualTo( 0 ) );

            Assert.That(
                () => buffer.Pop(),
                Throws.InvalidOperationException
                .With.Message.EqualTo( "Buffer is empty." ) );
        }

        /// <summary>
        /// Tests <see cref="CircularBuffer{TValue}.Push(TValue)"/> and
        /// <see cref="CircularBuffer{TValue}.Pop"/> with internal wrapping
        /// of the buffer (value overwrite).
        /// </summary>
        [Test]
        public void PushPopOverwriteTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );
            buffer.Push( 4 );
            buffer.Push( 5 );
            buffer.Push( 6 );

            Assert.That( buffer.Count, Is.EqualTo( 3 ) );
            Assert.That( buffer.Pop(), Is.EqualTo( 4 ) );
            Assert.That( buffer.Pop(), Is.EqualTo( 5 ) );
            Assert.That( buffer.Pop(), Is.EqualTo( 6 ) );

            Assert.That(
                () => buffer.Pop(),
                Throws.InvalidOperationException
                .With.Message.EqualTo( "Buffer is empty." ) );
        }

        /// <summary>
        /// Tests <see cref="CircularBuffer{TValue}.Peek"/>.
        /// </summary>
        [Test]
        public void PeekTest() {
            var buffer = new CircularBuffer<int>( 3 );

            Assert.That(
                () => buffer.Peek(),
                Throws.InvalidOperationException
                .With.Message.EqualTo( "Buffer is empty." ) );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            Assert.That( buffer.Peek(), Is.EqualTo( 1 ) );
            Assert.That( buffer.Peek(), Is.EqualTo( buffer.Peek() ) );

            buffer.Push( 4 );
            buffer.Push( 5 );
            buffer.Push( 6 );

            Assert.That( buffer.Peek(), Is.EqualTo( 4 ) );
            Assert.That( buffer.Peek(), Is.EqualTo( buffer.Peek() ) );

            buffer.Pop();

            Assert.That( buffer.Peek(), Is.EqualTo( 5 ) );
            Assert.That( buffer.Peek(), Is.EqualTo( buffer.Peek() ) );

            buffer.Pop();

            Assert.That( buffer.Peek(), Is.EqualTo( 6 ) );
            Assert.That( buffer.Peek(), Is.EqualTo( buffer.Peek() ) );

            buffer.Pop();

            Assert.That(
                () => buffer.Peek(),
                Throws.InvalidOperationException
                .With.Message.EqualTo( "Buffer is empty." ) );
        }

        /// <summary>
        /// Tests <see cref="CircularBuffer{TValue}.CopyTo(TValue[], int)"/>.
        /// </summary>
        [Test]
        public void CopyToTest() {
            var buffer = new CircularBuffer<int>( 3 );
            var destination = new int[] { 0, 0, 0 };

            Assert.That(
                () => buffer.CopyTo( null, 0 ),
                Throws.ArgumentNullException );

            Assert.That(
                () => buffer.CopyTo( destination, -1 ),
                Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Message.Contain( "Offset must be greater or equal to 0." ) );

            Assert.That(
                () => buffer.CopyTo( destination, 4 ),
                Throws.ArgumentException
                .With.Message.Contain( "No room in the destination array." ) );

            Assert.That(
                () => ( ( ICollection )buffer ).CopyTo( null, 0 ),
                Throws.ArgumentNullException );

            Assert.That(
                () => ( ( ICollection )buffer ).CopyTo( destination, -1 ),
                Throws.InstanceOf<ArgumentOutOfRangeException>()
                .With.Message.Contain( "Offset must be greater or equal to 0." ) );

            Assert.That(
                () => ( ( ICollection )buffer ).CopyTo( destination, 4 ),
                Throws.ArgumentException
                .With.Message.Contain( "No room in the destination array." ) );

            buffer.CopyTo( destination, 0 );
            Assert.That( destination, Is.EquivalentTo( new int[ 3 ] ) );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            buffer.CopyTo( destination, 0 );
            Assert.That( destination, Is.EquivalentTo( new int[] { 1, 2, 3 } ) );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );

            buffer.Push( 4 );
            buffer.Push( 5 );
            buffer.Push( 6 );

            buffer.CopyTo( destination, 0 );
            Assert.That( destination, Is.EquivalentTo( new int[] { 4, 5, 6 } ) );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );

            // Interface test
            buffer.Push( 7 );
            buffer.Push( 8 );
            buffer.Push( 9 );

            ( ( ICollection )buffer ).CopyTo( destination, 0 );
            Assert.That( destination, Is.EquivalentTo( new int[] { 7, 8, 9 } ) );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );
        }

        /// <summary>
        /// Tests the <see cref="CircularBuffer{TValue}.Enumerator"/>
        /// with an empty buffer.
        /// </summary>
        [Test]
        public void EnumeratorEmptyTest() {
            var buffer = new CircularBuffer<int>( 3 );

            using ( var enumerator = buffer.GetEnumerator() ) {
                for ( int i = 0; i < 2; i += 1 ) {
                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That(
                        () => ( ( IEnumerator )enumerator ).Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That( enumerator.MoveNext(), Is.False );

                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    Assert.That(
                        () => ( ( IEnumerator )enumerator ).Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    Assert.That( enumerator.MoveNext(), Is.False );

                    enumerator.Reset();
                }
            }
        }

        /// <summary>
        /// Tests <see cref="CircularBuffer{TValue}.Enumerator"/>.
        /// </summary>
        [Test]
        public void EnumeratorTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            Assert.That( buffer.GetEnumerator(), Is.Not.SameAs( buffer.GetEnumerator() ) );
            Assert.That( ( ( IEnumerable )buffer ).GetEnumerator(), Is.EqualTo( buffer.GetEnumerator() ) );

            using ( var enumerator = buffer.GetEnumerator() ) {
                for ( int i = 0; i < 2; i += 1 ) {
                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That(
                        () => ( ( IEnumerator )enumerator ).Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 1 ) );
                    Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 2 ) );
                    Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 3 ) );
                    Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );

                    Assert.That( enumerator.MoveNext(), Is.False );

                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    Assert.That(
                        () => ( ( IEnumerator )enumerator ).Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    enumerator.Reset();
                }
            }
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );
        }

        /// <summary>
        /// Tests <see cref="CircularBuffer{TValue}.Enumerator"/> with
        /// a wrapped buffer (overwritten values).
        /// </summary>
        [Test]
        public void EnumeratorFragmentedTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );

            buffer.Pop();
            buffer.Pop();
            Assert.That( buffer.Count, Is.EqualTo( 1 ) );

            buffer.Push( 4 );
            buffer.Push( 5 );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );

            using ( var enumerator = buffer.GetEnumerator() ) {
                for ( int i = 0; i < 2; i += 1 ) {
                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That(
                        () => ( ( IEnumerator )enumerator ).Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 3 ) );
                    Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 4 ) );
                    Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 5 ) );
                    Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );

                    Assert.That( enumerator.MoveNext(), Is.False );

                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    Assert.That(
                        () => ( ( IEnumerator )enumerator ).Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    enumerator.Reset();
                }
            }
        }

        /// <summary>
        /// Tests the <see cref="CircularBuffer{TValue}.Enumerator"/>
        /// while the items in the buffer are changing.
        /// </summary>
        [Test]
        public void EnumeratorChangingTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            using ( var enumerator = buffer.GetEnumerator() ) {
                Assert.That( enumerator.MoveNext(), Is.True );

                buffer.Pop();

                Assert.That(
                    () => enumerator.MoveNext(),
                    Throws.InvalidOperationException
                    .With.Message.EqualTo(
                        "Buffer changed during enumeration." ) );

                Assert.That(
                    () => enumerator.Reset(),
                    Throws.InvalidOperationException
                    .With.Message.EqualTo(
                        "Buffer changed during enumeration." ) );

                Assert.That( enumerator.Current, Is.EqualTo( 1 ) );
                Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );
            }

            using ( var enumerator = buffer.GetEnumerator() ) {
                Assert.That( enumerator.MoveNext(), Is.True );

                buffer.Push( 4 );

                Assert.That(
                    () => enumerator.MoveNext(),
                    Throws.InvalidOperationException
                    .With.Message.EqualTo(
                        "Buffer changed during enumeration." ) );

                Assert.That(
                    () => enumerator.Reset(),
                    Throws.InvalidOperationException
                    .With.Message.EqualTo(
                        "Buffer changed during enumeration." ) );

                Assert.That( enumerator.Current, Is.EqualTo( 2 ) );
                Assert.That( ( ( IEnumerator )enumerator ).Current, Is.EqualTo( enumerator.Current ) );
            }
        }

        /// <summary>
        /// Tests foreach iteration over the buffer.
        /// </summary>
        [Test]
        public void IterationTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            for ( int i = 0; i < 2; i += 1 ) {
                var x = 1;
                foreach ( var value in buffer ) {
                    Assert.That( value, Is.EqualTo( x ) );
                    x += 1;
                }
            }
        }

        /// <summary>
        /// Tests iteration over the buffer with calling
        /// <see cref="CircularBuffer{TValue}.Peek"/>. This
        /// makes sure that calls to peek actually don't change
        /// the underlying buffer.
        /// </summary>
        [Test]
        public void IterationPeekTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            for ( int i = 0; i < 2; i += 1 ) {
                var x = 1;
                foreach ( var value in buffer ) {
                    Assert.That( value, Is.EqualTo( x ) );
                    x += 1;

                    Assert.That( buffer.Peek(), Is.EqualTo( 1 ) );
                }
            }
        }
    }
}
