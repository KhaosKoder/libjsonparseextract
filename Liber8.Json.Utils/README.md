# Liber8.Json.Utils

A high-performance .NET 9+ JSON utility library designed to transform complex JSON objects into simpler ones based on configurable field mappings.

## Features

- **Field Extraction**: Extract values from complex JSON using configurable paths
- **Type Conversion**: Convert extracted values to specified target types
- **Action-Based Processing**: Support different JSON structures based on an action type field
- **Field Mapping Configuration**: Define output field names, source paths, and default values
- **Array Processing**: Support for array flattening/explosion and preservation
- **Error Handling**: Configurable error strategies with detailed error information
- **High Performance**: Optimized for high-volume, high-frequency JSON processing
- **Extensibility**: Well-defined interfaces for extensions and customization

## Installation

```bash
dotnet add package Liber8.Json.Utils
```

## Quick Start

```csharp
using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Core;
using Liber8.Json.Utils.Configuration;

// Create a configuration registry
var configRegistry = ConfigurationRegistry.Create();

// Create a configuration for the "CreateOrder" action type
var createOrderConfig = new ActionConfigurationBuilder("CreateOrder")
    .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
    .AddField("CustomerId", "$.OrderDetails.CustomerId", typeof(string))
    .AddField("OrderDate", "$.OrderDetails.OrderDate", typeof(DateTime))
    .AddField("TotalAmount", "$.OrderDetails.TotalAmount", typeof(decimal))
    .Build();

// Register the configuration
configRegistry.RegisterConfiguration(createOrderConfig);

// Create a JSON transformer
var transformer = JsonTransformer.Create(configRegistry);

// Transform a JSON string
var jsonString = @"
{
    ""Action"": ""CreateOrder"",
    ""OrderDetails"": {
        ""OrderId"": ""12345"",
        ""CustomerId"": ""CUST-001"",
        ""OrderDate"": ""2025-04-24T12:00:00Z"",
        ""TotalAmount"": 47.76
    }
}";

var results = transformer.Transform(jsonString);

// Process the results
foreach (var result in results)
{
    Console.WriteLine(result.ToString(Newtonsoft.Json.Formatting.Indented));
}
```

## Field Mapping Configuration

Field mappings define how values are extracted from the source JSON and added to the output JSON.

```csharp
// Basic field mapping
.AddField("OutputFieldName", "$.SourcePath", typeof(string))

// Field mapping with multiple source paths (first non-null value is used)
.AddField("OutputFieldName", new[] { "$.PrimaryPath", "$.FallbackPath" }, typeof(string))

// Field mapping with default value
.AddField("OutputFieldName", "$.SourcePath", typeof(string), "Default Value")

// Field mapping with omit if not found
.AddField("OutputFieldName", "$.SourcePath", typeof(string), null, true)

// Array explosion field mapping
.AddArrayExplosionField("Items", "$.OrderDetails.Items", new[] { "OrderId", "CustomerId" })
```

## Error Handling

The library provides configurable error handling strategies.

```csharp
// Create a configuration with error handling options
var config = new ActionConfigurationBuilder("CreateOrder")
    .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
    .AddField("OrderDate", "$.OrderDetails.OrderDate", typeof(DateTime))
    .WithFailFast(false) // Don't fail on the first error
    .WithIgnoreErrorsForFields(new[] { "OrderDate" }) // Ignore errors for specific fields
    .Build();

// Create a custom error handler
var errorHandler = ErrorHandler.Create(false);

// Create a parser engine with the custom error handler
var parserEngine = DefaultParserEngine.Create(
    pathResolver: JsonPathResolver.Create(),
    typeConverter: DefaultTypeConverter.Create(),
    errorHandler: errorHandler);

// Create a JSON transformer with the custom parser engine
var transformer = JsonTransformer.Create(configRegistry, parserEngine);

// Access errors after transformation
foreach (var error in errorHandler.Errors)
{
    Console.WriteLine($"Error: {error}");
}
```

## Advanced Usage

### Array Processing

The library supports two modes of array processing:

1. **Array Explosion**: Convert a single input message with an array into multiple output messages (one per array item)
2. **Array Preservation**: Maintain array structure while simplifying elements within it

```csharp
// Array explosion
var config = new ActionConfigurationBuilder("CreateOrder")
    .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
    .AddArrayExplosionField("Items", "$.OrderDetails.Items", new[] { "OrderId" })
    .Build();

// Array preservation
var config = new ActionConfigurationBuilder("CreateOrder")
    .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
    .AddField("Items", "$.OrderDetails.Items")
    .WithPreserveArrays(true)
    .Build();
```

### Custom Type Conversion

You can register custom type converters for specific types.

```csharp
var typeConverter = DefaultTypeConverter.Create();

// Register a custom converter for a specific type
typeConverter.RegisterConverter<CustomType>(token => 
{
    // Custom conversion logic
    return new CustomType { Property = token.Value<string>() };
});
```

## Performance Considerations

- The library is optimized for high-volume, high-frequency JSON processing
- Path expressions are compiled and cached for faster resolution
- Object pooling is used for frequently used objects
- Span-based operations are used where applicable

## License

This library is licensed under the MIT License - see the LICENSE file for details.
