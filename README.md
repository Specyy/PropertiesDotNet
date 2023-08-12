# PropertiesDotNet
A powerful `.properties` document parser for .NET

## What is .properties?
`.properties` is an informal configuration format mainly used in Java-related technologies to store a collection of application properties. It is a simple storage format that is only defined within the Javadoc documentation for the `Properties` class.

Each line in a .properties document normally stores a single property with an assigner delimeting the key and value (e.g. `key=value`). The format also supports comments with the handles `#` and `!` (e.g. `# This is a comment`)

## Overview
This powerful and intuitive library is designed to effortlessly parse Java `.properties` documents, providing a seamless integration into your .NET projects. Whether you're dealing with configuration files, localization data, or any other structured key-value pair data, PropertiesDotNet has got you covered. With its simple API and comprehensive and extensive features, you'll be up and running in no time.

The library allows for the configuration of .properties documents in three distinct ways: a low level token parsing and document writing API, a high level object model similar to XmlDocument, and a serialization library that allows for serialization and deserialization of .NET objects to and from .properties documents.

The library is currently available in the following .NET frameworks:
 
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

Install-Package PropertiesDotNet

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
using PropertiesDotNet.Serialization

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
```

## Installation
You can install the PropertiesDotNet library via NuGet Package Manager or by using the following NuGet CLI command:
```bash
nuget install PropertiesDotNet
