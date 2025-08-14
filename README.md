# PropertiesDotNet

A powerful, fully-featured `.properties` document parser for .NET

## What is .properties?
`.properties` is an informal configuration format mainly used in Java-related technologies to store collections of application properties. It is a simple (yet much more involved than is commonly thought) storage format that is only defined within the [Javadoc documentation](https://docs.oracle.com/javase/8/docs/api/java/util/Properties.html#load-java.io.Reader-) for the `java.util.Properties` class.

## Overview
PropertiesDotNet is a helpful tool tailored for developers who are transitioning from Java to .NET. It addresses the challenge of working with Java's `.properties` files within the .NET environment. This library is designed to effortlessly parse Java `.properties` documents, providing a seamless integration into your .NET projects.

We do this by allowing for the configuration of .properties documents in three distinct ways: a low-level token-parsing and document writing API, a high level object model similar to XmlDocument, and a serialization library for the serialization and deserialization of .NET objects to and from .properties documents.

PropertiesDotNet is currently available in the following .NET runtimes:
 
* .NET 8.0 
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

## Documentation

Full documentation for the API is available on the [wiki](https://github.com/Specyy/PropertiesDotNet/wiki).

## Contribute

We welcome contributions to PropertiesDotNet. If you would like to contribute, please follow these steps:

## Fork the repository.

1. Create a new branch for your feature.
2. Implement your feature.
3. Add test cases for your feature.
4. Run the test cases and make sure they all pass.
5. Commit your changes and create a pull request.

## License
PropertiesDotNet is licensed under the MIT License.
