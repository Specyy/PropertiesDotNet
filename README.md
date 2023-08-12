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

## Installation
You can install the PropertiesDotNet library via NuGet Package Manager or by using the following NuGet CLI command:
```bash
nuget install PropertiesDotNet
