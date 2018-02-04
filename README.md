# DD.Collections.Generic.CircularBuffer

This is a single-file implementation of a generic fixed-size [Circular Buffer](https://en.wikipedia.org/wiki/Circular_buffer) data structure for the .NET Standard 1.6 and C# 7.2.

The implementation itself is located within a single *cs* [file](SharedCode/CircularBuffer.cs) for easy inclusion in your projects.

The implementation is tested via [NUnit](http://nunit.org). The tests can be found in the [CircularBuffer.Tests](CircularBuffer.Tests/CircularBuffer.Tests.csproj) project.

## Build

This is a single-file implementation and therefore doesn't need to be build beforehand. Include [CircularBuffer.cs](SharedCode/CircularBuffer.cs) into your project and make sure that you are targeting a .NET Standard 1.6 compatible runtime. To run the included tests execute `dotnet test`. This will run the unit tests with `netcoreapp2.0` and `net45` as target frameworks.

## Usage

The CircularBuffer class is located within the `DD.Collections.Generic` namespace. To create a new instance you simply have to call the constructor:

```cs
// Creates a buffer with room for 1023 items
var default_capacity_instance = new CircularBuffer<int>();

// Creates a buffer with room for 50 items
var defined_capacity_instance = new CircularBuffer<int>( 50 );
```

To add a new item at the end of the buffer you have to call `Push`. To remove the item at the beginning of the buffer you have to call `Pop`:

```cs
defined_capacity_instance.Push( 1 );
defined_capacity_instance.Push( 2 );
defined_capacity_instance.Push( 3 );

var a = defined_capacity_instance.Pop(); // a == 1
var b = defined_capacity_instance.Pop(); // b == 2
var c = defined_capacity_instance.Pop(); // c == 3
```

You can also enumerate over all items within the buffer as the circular buffer inherits `IReadOnlyCollection<T>` which in return inherits `IEnumerable<T>`. (**Iterating over the buffer does not consume its items**):

```cs
defined_capacity_instance.Push( 1 );
defined_capacity_instance.Push( 2 );
defined_capacity_instance.Push( 3 );

foreach ( var item in defined_capacity_instance ) {
    Console.WriteLine( item ); // Prints all items
}

if ( defined_capacity_instance.Count == 3 ) {
    Console.WriteLine( "Still got all items!" );
}
```

### License

See [LICENSE](LICENSE) file.
