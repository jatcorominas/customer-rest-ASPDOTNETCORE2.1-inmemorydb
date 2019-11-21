using Customers.Models;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class FakeHttpMessageHandler : DelegatingHandler
    {
        private HttpResponseMessage _fakeResponse;

        public FakeHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            _fakeResponse = responseMessage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_fakeResponse);
        }
    }

    public class UnitTests
    {
        [Fact]
        public async Task TestPost_ReturnsStatusCode201Created()
        {
            // Arrange
            var customerToAdd = new Customer { name = "Michael", age = 50, active = false };

            var expectedCustomer = new Customer { id = 1, name = "Michael", age = 50, active = false };

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(JsonConvert.SerializeObject(expectedCustomer), Encoding.UTF8, "application/json")
            });

            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(customerToAdd), Encoding.UTF8, "application/json"));
            string actualJsonCustomer = await response.Content.ReadAsStringAsync();
            Customer actualCustomer = new Customer();
            JsonConvert.PopulateObject(actualJsonCustomer, actualCustomer);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            actualCustomer
               .Should()
               .BeOfType<Customer>()
               .And
               .Equals(expectedCustomer);

        }

        [Fact]
        public async Task TestPost_ReturnsStatusCode401NotFound()
        {
            // Arrange
            var customerToAdd = new Customer { name = "Michael", age = 50, active = false };

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound
            });

            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(customerToAdd), Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }

        [Fact]
        public async Task TestPost_ReturnsStatusCode500ServerError()
        {
            // Arrange
            var customerToAdd = new Customer { name = "Michael", age = 50, active = false };

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(customerToAdd), Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        }

        [Fact]
        public async Task TestPutCustomer_ReturnsOK()
        {
            // Arrange
            var idOfCustomerToUpdate = 1;
            var customerUpdateDetails = new Customer{ id = 1, name = "Michael", age = 70, active = true };
            var expectedContent = "Successfully Updated";
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/1";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Successfully Updated", Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(customerUpdateDetails), Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().Equals(expectedContent);
        }

        [Fact]
        public async Task TestPutCustomer_ReturnsStatusCode404NotFound()
        {
            // Arrange
            var idOfCustomerToUpdate = 3;
            var customerUpdateDetails = new Customer { id = 3, name = "Michael", age = 70, active = true };
            
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/3";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound
               
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(customerUpdateDetails), Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            
        }

        [Fact]
        public async Task TestPutCustomer_ReturnsStatusCode500InternalServerError()
        {
            // Arrange
            var idOfCustomerToUpdate = 3;
            var customerUpdateDetails = new Customer { id = 3, name = "Michael", age = 70, active = true };

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/3";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,

            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(customerUpdateDetails), Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        }

        [Fact]
        public async Task TestGet_ReturnsOK()
        {

            // Arrange
            var customers = new List<Customer>()
            {
                new Customer { id=1, name="Barney", age=25, active=false},
                new Customer { id=2, name="Alice", age=23, active=false}
            };

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(customers), Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.GetAsync(url);
            string actualJsonCustomers = await response.Content.ReadAsStringAsync();
            List<Customer> actualCustomers = new List<Customer>();
            JsonConvert.PopulateObject(actualJsonCustomers, actualCustomers);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualCustomers
                .Should()
                .BeOfType<List<Customer>>()
                .And
                .HaveCount(2)
                .And
                .Contain(c => c.id == 1)
                .And
                .Contain(c => c.name.Equals("Barney"))
                .And
                .Contain(c => c.active == false)
                .And
                .Contain(c => c.age == 25)
                .And
                .Contain(c => c.id == 2)
                .And
                .Contain(c => c.name.Equals("Alice"))
                .And
                .Contain(c => c.active == false)
                .And
                .Contain(c => c.age == 23);

           
        }

        [Fact]
        public async Task TestGet_ReturnsStatusCode401NotFound()
        {
            // Arrange
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound,
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.GetAsync(url);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task TestGet_ReturnsStatusCode500InternalServerError()
        {
            // Arrange
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task TestGetById_ReturnsOK()
        {
            // Arrange
            Customer expectedCustomer = new Customer { id = 2, name = "Alice", age = 23, active = false };
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/2";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedCustomer), Encoding.UTF8, "application/json")
            });

            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.GetAsync(url);
            string actualJsonCustomer = await response.Content.ReadAsStringAsync();
            Customer actualCustomer = new Customer();
            JsonConvert.PopulateObject(actualJsonCustomer, actualCustomer);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualCustomer
               .Should()
               .BeOfType<Customer>()
               .And
               .Equals(expectedCustomer);

        }

        [Fact]
        public async Task TestGetById_Returns404NotFound()
        {
            // Arrange
            Customer expectedCustomer = new Customer { id = 2, name = "Alice", age = 23, active = false };
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/2";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound,
            });

            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.GetAsync(url);
           
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }

        [Fact]
        public async Task TestGetById_Returns500InternalServerError()
        {
            // Arrange
            Customer expectedCustomer = new Customer { id = 2, name = "Alice", age = 23, active = false };
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/2";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,
            });

            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        }


        [Fact]
        public async Task TestGetByAge_ReturnsOK()
        {
            // Arrange
            var expectedCustomers = new List<Customer>()
            {
                new Customer { id=1, name="Mabel", age=30, active=false},
                new Customer { id=2, name="Petra", age=30, active=false}
            };

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/age/30";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(expectedCustomers), Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);


            // Act
            HttpResponseMessage response = await fakeHttpClient.GetAsync(url);
            string actualJsonCustomers = await response.Content.ReadAsStringAsync();
            List<Customer> actualCustomers = new List<Customer>();
            JsonConvert.PopulateObject(actualJsonCustomers, actualCustomers);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualCustomers
                .Should()
                .BeOfType<List<Customer>>()
                .And
                .HaveCount(2)
                .And
                .Contain(c => c.id == 1)
                .And
                .Contain(c => c.name.Equals("Mabel"))
                .And
                .Contain(c => c.active == false)
                .And
                .Contain(c => c.age == 30)
                .And
                .Contain(c => c.id == 2)
                .And
                .Contain(c => c.name.Equals("Petra"))
                .And
                .Contain(c => c.active == false)
                .And
                .Contain(c => c.age == 30);
        }

        [Fact]
        public async Task TestDeleteCustomerById_ReturnsOK()
        {
            // Arrange
            string expectedContent = "Successfully deleted customer";
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/3";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Successfully deleted customer", Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.DeleteAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().Equals(expectedContent);

        }

        [Fact]
        public async Task TestDeleteCustomerById_Returns404NotFound()
        {
            // Arrange
            
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/3";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.NotFound,
               
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.DeleteAsync(url);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task TestDeleteCustomerById_Returns500InternalServerError()
        {
            // Arrange

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/3";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError,

            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.DeleteAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task TestDeleteAllCustomers_ReturnsOK()
        {
            // Arrange
            string expectedContent = "Deleted All Customers";
            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var url = "http://localhost:8090/api/customers/delete";
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Deleted All Customers", Encoding.UTF8, "application/json")
            });
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler);
            httpClientFactoryMock.CreateClient().Returns(fakeHttpClient);

            // Act
            HttpResponseMessage response = await fakeHttpClient.DeleteAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Should().Equals(expectedContent);

        }


    }
   
}
