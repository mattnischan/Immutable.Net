Immutable.Net
=============
######Current Build Status: [![Build status](https://ci.appveyor.com/api/projects/status/yu3syp2av197wk11)](https://ci.appveyor.com/project/mattnischan/immutable-net)

Immutable.Net is a lightweight library for using immutable datatypes as first class citizens in .Net (as much as the CLR will allow, anyway). Optimized for performance and size, it provides a simple API for creating and using immutable datatypes.

###Where To Get It
Immutable.Net is available as a package on [NuGet.org](http://www.nuget.org).

###Using Immutable.Net
Immutable.Net is very simple to use. At it's base, all you need to do is wrap your data type in an Immutable:
```
public class Order
{
  public int OrderId { get; set; }
  public string CustomerName { get; set; }
  public string Description { get; set; }
}

var order = new Immutable<Order>();
```
Then, you can alter your immutable data type like so:
```
var newOrder = order.Modify(x => x.OrderId, 1);
```
This will create a shallow clone of your data type, modify the new instance, and return it. Don't worry, your original instance is safe:
```
Assert.NotEqual(order, newOrder); //Does not throw! :)
```
You can also chain the Modify method to build more complex data:
```
var newOrder = order.Modify(x => x.OrderId, 1)
  .Modify(x => x.CustomerName = "Art Vandelay")
  .Modify(x => x.Description = "Drafting supplies");
```
However, doing this too much causes pressure on the garbage collector, as each run of Modify creates a new instance that is, in this case, immediately discarded until the final invocation. To help combat this, Immutable.Net provider a builder class that is mutable.
```
var newOrder = order.ToBuilder()
  .Modify(x => x.OrderId = 1)
  .Modify(x => x.CustomerName = "Art Vandelay")
  .Modify(x => x.Description = "Drafting supplies")
  .ToImmutable();
```
Note that the ImmutableBuilder's Modify method has a slightly different signature than the Immutable version in order to allow direct and speedy access to the enclosed object's members.

###What About Speed?
Immutable.Net builds setter and cloning IL using Expressions and caches those strongly typed delegates. This means that it is quite fast overall. Some memory is required to store the delegate references, but this overhead is not large. Like other libraries that use a similar strategy, the first call is expensive as the IL is built, but subsequent calls are quite cheap.

Immutable.Net does abstract away many of the headaches of maintaining immutable data types (i.e. copy constructors, builder versions of each type, etc.), but if you are in an extremely performance sensitive environment (real time write heavy data analysis, gaming), handrolling your immutable types may be faster. Remember, performance is a feature, premature optimization is evil!

###Caveats
Data types that are wrapped using Immutable and ImmutableBuilder must have parameterless constructors. This may change in the future.

###License
Immutable.Net is licensed under the [Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0.html) license.
