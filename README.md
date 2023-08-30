# PropertiesDotNet
A powerful, fully-featured `.properties` document parser for .NET

## What is .properties?
`.properties` is an informal configuration format mainly used in Java-related technologies to store collections of application properties. It is a simple (yet much more involved than is commonly thought) storage format that is only defined within the [Javadoc documentation](https://docs.oracle.com/javase/8/docs/api/java/util/Properties.html#load-java.io.Reader-) for the `java.util.Properties` class.

## Overview
PropertiesDotNet is a helpful tool tailored for developers who are transitioning from Java to .NET. It addresses the challenge of working with Java's `.properties` files within the .NET environment. This tool fills the gap by offering a user-friendly API that simplifies the interaction with `.properties` documents.

The API provided by this library serves a variety of functions. It facilitates efficient reading and writing of `.properties` documents using token-based identification, and it comes with built-in error handling and validation capabilities, yielding performance that surpasses that of other libraries, including even the Java standard library.

It also supports the creation of a document object model through the `PropertiesDocument` implementation. This model resembles Java's `Properties` class and the familiar `XmlDocument`. This enables developers to work with `.properties` files in a structured and intuitive manner.

Furthermore, PropertiesDotNet empowers developers with the ability to serialize and deserialize .NET objects (trees) to and from `.properties` documents. This functionality streamlines the process of converting complex data structures into a format that can be easily stored in `.properties` files and vice versa.

The library is currently available in the following .NET runtimes:
 
* .NET 6.0 
* .NET Core 3.0
* .NET Standard 2.1
* .NET Standard 2.0
* .NET Standard 1.3
* .NET Framework 4.6.1
* .NET Framework 4.0
* .NET Framework 3.5

## Quick Start

1. Install the package from NuGet:
```bash
Install-Package PropertiesDotNet
```

2. Use the `PropertiesDocument` class to load a .properties file:

```csharp
using PropertiesDotNet.ObjectModel;

string filePath = "path/to/file.properties"
var properties = PropertiesDocument.Load(filePath);
```

3. Access and update the properties:

```csharp
// Get a property
string? propertyValue = properties["property.name"];

// Set a property
properties["property.name"] = "new value";

// Save the properties back to the .properties file
properties.Save(filePath);
```

## Reading and writing .properties

1. Create a new `PropertiesReader`

```csharp
using PropertiesDotNet.Core;

var reader = new PropertiesReader(filePath);
```

2. Read the document by tokens 

```csharp
while(reader.MoveNext())
{
    var token = reader.Token;
    
    switch(token.Type)
    {
        // A document comment
        case PropertiesTokenType.Comment:
            break;

        // A property key
        case PropertiesTokenType.Key:
            break;

        // A property assigner/delimiter (=,:,\f or \x20)
        case PropertiesTokenType.Assigner:
            break;

        // A property value
        case PropertiesTokenType.Value:
            break;

        // An error occurred while parsing the document
        case PropertiesTokenType.Error:
            break;
    }
}
```

## Serialization

```csharp
using PropertiesDotNet.Serialization;

public class Player
{
    public string Name { get; set; }
    public int Age { get; set; }
    public List<Player> Friends { get; set; }

    public override string? ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Name: {Name}");
        sb.AppendLine($"Age: {Age}");
        sb.AppendLine($"Friends:");

        foreach(var friend in Friends)
        {
            sb.AppendLine($"    - Name: {friend.Name}");
            sb.AppendLine($"    - Age: {friend.Age}");
        }

        return sb.ToString();
    }
}

// path/to/file.properties
//
// Name = Steven
// Age = 36
// Friends.0.Name = Albert
// Friends.0.Age = 43
// Friends.1.Name = Timmy
// Friends.1.Age = 22

string filePath = "path/to/file.properties";
var player = PropertiesSerializer.Deserialize<Player>(filePath);

Console.WriteLine(player);

// Output:
// 
// Name: Steven
// Age: 36
// Friends:
//     - Name: Albert
//     - Age: 43
//     - Name: Timmy
//     - Age: 22
//
```

# Pending features
The following features could potentially be implemented in future versions of the library:
* Circular references during (de)serialization
* Property referencing for readers and (de)serialization (`person.fullName = ${person.firstName} ${person.lastName}`)


## Installation
You can install the PropertiesDotNet library via NuGet Package Manager or by using the following NuGet CLI command:
```bash
nuget install PropertiesDotNet
```
