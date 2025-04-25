using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Liber8.Json.Utils.Core;
using Liber8.Json.Utils.Configuration;
using Liber8.Json.Utils.Parsing;
using Liber8.Json.Utils.Extraction;
using Liber8.Json.Utils.Conversion;
using Liber8.Json.Utils.ErrorHandling;
using System.Linq;

namespace Liber8.Json.Utils.Tests
{
    [TestClass]
    public class JsonTransformerTests
    {
        [TestMethod]
        public void Transform_SimpleJson_ReturnsExpectedResult()
        {
            // Arrange
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

            var configRegistry = ConfigurationRegistry.Create();
            var createOrderConfig = new ActionConfigurationBuilder("CreateOrder")
                .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
                .AddField("CustomerId", "$.OrderDetails.CustomerId", typeof(string))
                .AddField("OrderDate", "$.OrderDetails.OrderDate", typeof(System.DateTime))
                .AddField("TotalAmount", "$.OrderDetails.TotalAmount", typeof(decimal))
                .Build();

            configRegistry.RegisterConfiguration(createOrderConfig);
            var transformer = JsonTransformer.Create(configRegistry);

            // Act
            var results = transformer.Transform(jsonString).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            var result = results[0];
            Assert.AreEqual("12345", result["OrderId"]?.ToString());
            Assert.AreEqual("CUST-001", result["CustomerId"]?.ToString());
            Assert.AreEqual("2025-04-24T12:00:00Z", result["OrderDate"]?.ToString());
            Assert.AreEqual(47.76m, result["TotalAmount"]?.Value<decimal>());
        }

        [TestMethod]
        public void Transform_WithArrayExplosion_ReturnsMultipleResults()
        {
            // Arrange
            var jsonString = @"
            {
                ""Action"": ""CreateOrder"",
                ""OrderDetails"": {
                    ""OrderId"": ""12345"",
                    ""CustomerId"": ""CUST-001"",
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
                    ]
                }
            }";

            var configRegistry = ConfigurationRegistry.Create();
            var createOrderConfig = new ActionConfigurationBuilder("CreateOrder")
                .AddField("OrderId", "$.OrderDetails.OrderId", typeof(string))
                .AddField("CustomerId", "$.OrderDetails.CustomerId", typeof(string))
                .AddArrayExplosionField("Items", "$.OrderDetails.Items", new[] { "OrderId", "CustomerId" })
                .Build();

            configRegistry.RegisterConfiguration(createOrderConfig);
            var transformer = JsonTransformer.Create(configRegistry);

            // Act
            var results = transformer.Transform(jsonString).ToList();

            // Assert
            Assert.AreEqual(2, results.Count);
            
            // First item
            Assert.AreEqual("12345", results[0]["OrderId"]?.ToString());
            Assert.AreEqual("CUST-001", results[0]["CustomerId"]?.ToString());
            Assert.AreEqual("PROD-001", results[0]["ProductId"]?.ToString());
            Assert.AreEqual("Widget A", results[0]["Name"]?.ToString());
            Assert.AreEqual(2, results[0]["Quantity"]?.Value<int>());
            Assert.AreEqual(10.99, results[0]["Price"]?.Value<double>());
            
            // Second item
            Assert.AreEqual("12345", results[1]["OrderId"]?.ToString());
            Assert.AreEqual("CUST-001", results[1]["CustomerId"]?.ToString());
            Assert.AreEqual("PROD-002", results[1]["ProductId"]?.ToString());
            Assert.AreEqual("Widget B", results[1]["Name"]?.ToString());
            Assert.AreEqual(1, results[1]["Quantity"]?.Value<int>());
            Assert.AreEqual(15.99, results[1]["Price"]?.Value<double>());
        }
    }
}
