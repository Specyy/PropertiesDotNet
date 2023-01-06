# PropertiesDotNet

Parses Java .properties documents and provides an easy-to-use API for interacting with the data.

## Features

- Parses .properties documents and converts them into a .NET object model
- Provides a simple API for accessing and updating the properties in the object model
- Supports comments in the .properties document

## Getting Started

1. Install the package from NuGet:

Install-Package PropertiesDotNet

2. Use the `Properties` class to load a .properties file:

```csharp
using PropertiesDotNet;

string filePath = "path/to/file.properties";
Properties properties = Properties.Load(filePath);
```

3. Access and update the properties:

```csharp
// Get a property
string propertyValue = properties.GetProperty("property.name");

// Set a property
properties.SetProperty("property.name", "new value");

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
