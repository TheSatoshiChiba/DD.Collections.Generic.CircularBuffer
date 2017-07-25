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

using NUnit.Framework;
using System;

namespace DD.Collections.Generic.Tests {
    /// <summary>
    /// The <see cref="CircularBuffer{TValue}"/> tests.
    /// </summary>
    public class CircularBufferTests {
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

            buffer.CopyTo( destination, 0 );
            Assert.That( destination, Is.EquivalentTo( new int[ 3 ] ) );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            buffer.CopyTo( destination, 0 );
            Assert.That( destination, Is.EquivalentTo( new int[] { 1, 2, 3 } ) );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );

            buffer.Pop();
            buffer.Pop();
            buffer.Pop();

            buffer.Push( 4 );
            buffer.Push( 5 );
            buffer.Push( 6 );

            buffer.CopyTo( destination, 0 );
            Assert.That( destination, Is.EquivalentTo( new int[] { 4, 5, 6 } ) );
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );
        }

        [Test]
        public void EnumeratorEmptyTest() {
            var buffer = new CircularBuffer<int>( 3 );

            using ( var enumerator = buffer.GetEnumerator() ) {
                for ( int i = 0; i < 2; i += 1 ) {
                    Assert.That(
                    () => enumerator.Current,
                    Throws.InvalidOperationException
                    .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That( enumerator.MoveNext(), Is.False );

                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    Assert.That( enumerator.MoveNext(), Is.False );

                    enumerator.Reset();
                }
            }
        }

        [Test]
        public void EnumeratorTest() {
            var buffer = new CircularBuffer<int>( 3 );

            buffer.Push( 1 );
            buffer.Push( 2 );
            buffer.Push( 3 );

            Assert.That( buffer.GetEnumerator(), Is.Not.SameAs( buffer.GetEnumerator() ) );

            using ( var enumerator = buffer.GetEnumerator() ) {
                for ( int i = 0; i < 2; i += 1 ) {
                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration not started." ) );

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 1 ) );
                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 2 ) );
                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 3 ) );
                    Assert.That( enumerator.MoveNext(), Is.False );

                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    enumerator.Reset();
                }
            }
            Assert.That( buffer.Count, Is.EqualTo( 3 ) );
        }

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

                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 3 ) );
                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 4 ) );
                    Assert.That( enumerator.MoveNext(), Is.True );
                    Assert.That( enumerator.Current, Is.EqualTo( 5 ) );
                    Assert.That( enumerator.MoveNext(), Is.False );

                    Assert.That(
                        () => enumerator.Current,
                        Throws.InvalidOperationException
                        .With.Message.EqualTo( "Enumeration finished." ) );

                    enumerator.Reset();
                }
            }
        }

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
            }
        }

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
    }
}
