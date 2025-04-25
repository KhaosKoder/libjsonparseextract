using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Core;
using Liber8.Json.Utils.Configuration;
using Liber8.Json.Utils.Parsing;
using Liber8.Json.Utils.Extraction;
using Liber8.Json.Utils.Conversion;
using Liber8.Json.Utils.ErrorHandling;

namespace Liber8.Json.Utils.Examples
{
    /// <summary>
    /// Provides a simple example of how to use the Liber8.Json.Utils library.
    /// </summary>
    public static class SimpleExample
    {
        /// <summary>
        /// Runs the simple example.
        /// </summary>
        public static void Run()
        {
            // Create a sample JSON string
            var jsonString = @"
            {
                ""Action"": ""CreateOrder"",
                ""OrderDetails"": {
                    ""OrderId"": ""12345"",
                    ""CustomerId"": ""CUST-001"",
                    ""OrderDate"": ""2025-04-24T12:00:00Z"",
                    ""Items"": [
                        {
                            ""ProductId"": ""PROD-001"",
                            ""Name"": ""Widget A"",
                            ""Quantity"": 2,
                            ""Price"": 10.99
                        },
                        {
                            ""ProductId"": ""PROD-002"",
                            ""Name"": ""Widget B"",
                            ""Quantity"": 1,
                            ""Price"": 15.99
                        }
                    ],
                    ""ShippingAddress"": {
                        ""Street"": ""123 Main St"",
                        ""City"": ""Anytown"",
                        ""State"": ""CA"",
                        ""ZipCode"": ""12345""
                    },
                    ""BillingAddress"": {
                        ""Street"": ""123 Main St"",
                        ""City"": ""Anytown"",
                        ""State"": ""CA"",
                        ""ZipCode"": ""12345""
                    },
                    ""PaymentMethod"": {
                        ""Type"": ""CreditCard"",
                        ""CardNumber"": ""**** **** **** 1234"",
                        ""ExpirationDate"": ""12/25"",
                        ""CardholderName"": ""John Doe""
                    },
                    ""TotalAmount"": 37.97,
                    ""Tax"": 3.80,
                    ""ShippingCost"": 5.99,
                    ""Discount"": 0.00,
                    ""GrandTotal"": 47.76,
                    ""Status"": ""Pending"",
                    ""Notes"": ""Please deliver to the back door.""
                },
                ""Metadata"": {
                    ""Source"": ""Web"",
                    ""IpAddress"": ""192.168.1.1"",
                    ""UserAgent"": ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"",
                    ""Timestamp"": ""2025-04-24T12:00:00Z""
                }
            }";

            // Create a configuration registry
            var configRegistry = ConfigurationRegistry.Create();

            // Create a configuration for the "CreateOrder" action type
            var createOrderConfig = new ActionConfigurationBuilder("CreateOrder")
                // Basic order information
                .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
                .AddField("CustomerId", "$.OrderDetails.CustomerId", typeof(string))
                .AddField("OrderDate", "$.OrderDetails.OrderDate", typeof(DateTime))
                .AddField("Status", "$.OrderDetails.Status", typeof(string))
                
                // Financial information
                .AddField("TotalAmount", "$.OrderDetails.TotalAmount", typeof(decimal))
                .AddField("Tax", "$.OrderDetails.Tax", typeof(decimal))
                .AddField("ShippingCost", "$.OrderDetails.ShippingCost", typeof(decimal))
                .AddField("Discount", "$.OrderDetails.Discount", typeof(decimal))
                .AddField("GrandTotal", "$.OrderDetails.GrandTotal", typeof(decimal))
                
                // Shipping address (flattened)
                .AddField("ShippingStreet", "$.OrderDetails.ShippingAddress.Street", typeof(string))
                .AddField("ShippingCity", "$.OrderDetails.ShippingAddress.City", typeof(string))
                .AddField("ShippingState", "$.OrderDetails.ShippingAddress.State", typeof(string))
                .AddField("ShippingZipCode", "$.OrderDetails.ShippingAddress.ZipCode", typeof(string))
                
                // Payment information (selected fields only)
                .AddField("PaymentType", "$.OrderDetails.PaymentMethod.Type", typeof(string))
                .AddField("PaymentCardLast4", "$.OrderDetails.PaymentMethod.CardNumber", typeof(string))
                
                // Metadata
                .AddField("Source", "$.Metadata.Source", typeof(string))
                .AddField("Timestamp", "$.Metadata.Timestamp", typeof(DateTime))
                
                // Notes (with default value if not found)
                .AddField("Notes", "$.OrderDetails.Notes", typeof(string), "No notes provided", true)
                
                // Array explosion for order items
                .AddArrayExplosionField("Items", "$.OrderDetails.Items", new[] { "OrderId", "CustomerId" })
                
                .Build();

            // Register the configuration
            configRegistry.RegisterConfiguration(createOrderConfig);

            // Create a JSON transformer
            var transformer = JsonTransformer.Create(configRegistry);

            // Transform the JSON string
            var results = transformer.Transform(jsonString).ToList();

            // Print the results
            Console.WriteLine("Transformed JSON objects:");
            foreach (var result in results)
            {
                Console.WriteLine(result.ToString(Newtonsoft.Json.Formatting.Indented));
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Demonstrates how to handle errors during transformation.
        /// </summary>
        public static void RunWithErrorHandling()
        {
            // Create a sample JSON string with errors
            var jsonString = @"
            {
                ""Action"": ""CreateOrder"",
                ""OrderDetails"": {
                    ""OrderId"": ""12345"",
                    ""CustomerId"": ""CUST-001"",
                    ""OrderDate"": ""invalid-date"",
                    ""Items"": [
                        {
                            ""ProductId"": ""PROD-001"",
                            ""Name"": ""Widget A"",
                            ""Quantity"": ""not-a-number"",
                            ""Price"": 10.99
                        }
                    ],
                    ""TotalAmount"": ""not-a-decimal""
                }
            }";

            // Create a configuration registry
            var configRegistry = ConfigurationRegistry.Create();

            // Create a configuration for the "CreateOrder" action type
            var createOrderConfig = new ActionConfigurationBuilder("CreateOrder")
                .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
                .AddField("CustomerId", "$.OrderDetails.CustomerId", typeof(string))
                .AddField("OrderDate", "$.OrderDetails.OrderDate", typeof(DateTime))
                .AddField("TotalAmount", "$.OrderDetails.TotalAmount", typeof(decimal))
                .AddArrayExplosionField("Items", "$.OrderDetails.Items", new[] { "OrderId", "CustomerId" })
                .WithFailFast(false) // Don't fail on the first error
                .WithIgnoreErrorsForFields(new[] { "OrderDate" }) // Ignore errors for OrderDate
                .Build();

            // Register the configuration
            configRegistry.RegisterConfiguration(createOrderConfig);

            // Create a custom error handler
            var errorHandler = ErrorHandler.Create(false);

            // Create a parser engine with the custom error handler
            var parserEngine = DefaultParserEngine.Create(
                pathResolver: JsonPathResolver.Create(),
                typeConverter: DefaultTypeConverter.Create(),
                errorHandler: errorHandler);

            // Create a JSON transformer
            var transformer = JsonTransformer.Create(configRegistry, parserEngine);

            try
            {
                // Transform the JSON string
                var results = transformer.Transform(jsonString).ToList();

                // Print the results
                Console.WriteLine("Transformed JSON objects:");
                foreach (var result in results)
                {
                    Console.WriteLine(result.ToString(Newtonsoft.Json.Formatting.Indented));
                    Console.WriteLine();
                }

                // Print the errors
                Console.WriteLine("Errors encountered during transformation:");
                foreach (var error in errorHandler.Errors)
                {
                    Console.WriteLine($"- {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
