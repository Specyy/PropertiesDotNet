# PropertiesDotNet
A powerful `.properties` document parser for .NET

## What is .properties?
`.properties` is a configuration format mainly used in Java-related technologies to store the parameters of an application. It is simple, informal storage format that is only defined within the `Properties` Javadoc documentation. 

Each line in a .properties file normally stores a single property with an assigner delimeting the key and value (e.g. `key=value`).

## Overview
This powerful and intuitive library is designed to effortlessly parse Java `.properties` documents, providing a seamless integration into your .NET projects. Whether you're dealing with configuration files, localization data, or any other structured key-value pair data, PropertiesDotNet has got you covered. With its simple API and comprehensive and extensive features, you'll be up and running in no time.

The library allows for the interaction with .properties documents in three distinct ways: a low level parsing and emitting API for writing documents, a high level object model similar to XmlDocument, and a serialization library that allows for serialization and deserialization of .NET objects to and from .properties documents.

## Installation
You can install the PropertiesDotNet library via NuGet Package Manager or by using the following NuGet CLI command:
```bash
nuget install PropertiesDotNet
