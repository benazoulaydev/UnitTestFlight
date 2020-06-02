using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlightSimulatorWebApi.Controllers;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using FlightSimulatorWebApi.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using FlightSimulatorWebApi;
using System.Threading;
using System.Diagnostics.Contracts;
using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

/// <summary>
/// UnitTestProject1 model
/// </summary>
namespace UnitTestFlight
{
    /// <summary>
    /// sstub object
    /// </summary>
    /// <seealso cref="System.Net.Http.DelegatingHandler" />
    public class DelegatingHandlerStub : DelegatingHandler
    {
        /// <summary>
        /// The handler function
        /// </summary>
        private readonly Func<HttpRequestMessage, CancellationToken, 
            Task<HttpResponseMessage>> _handlerFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingHandlerStub"/> class.
        /// </summary>
        public DelegatingHandlerStub()
        {
            //_handlerFunc = (request, cancellationToken) => Task.FromResult();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegatingHandlerStub"/> class.
        /// </summary>
        /// <param name="handlerFunc">The handler function.</param>
        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken,
            Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server 
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }
    }

    /// <summary>
    /// the unit test class of FlightSimulatorWebApi
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Empties the list test.
        /// </summary>
        [TestMethod]
        public void EmptyListTest()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);
            IHttpClientFactory factory = mockFactory.Object;

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            Mock<IMemoryCache> cache = new Mock<IMemoryCache>();
            FlightPlanController controller = new FlightPlanController(memoryCache, factory);
            // Arrange
            var myContext = new Mock<HttpContext>();
            myContext.SetupGet(x => x.Request.QueryString).
                Returns(new QueryString("?relative_to=" + DateTime.Now));
            var controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            controller = new FlightPlanController(memoryCache, factory)
            {
                ControllerContext = controllerContext,
            };
            // Act
            var actionResult = controller.GetFlightsByDateAsync(DateTime.Now);

            // Assert
            Assert.IsTrue(actionResult.Result.Value.Count == 0);
        }

        /// <summary>
        /// Wrongs the fligth identifier.
        /// </summary>
        [TestMethod]
        public void WrongFligthID()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            Mock<IMemoryCache> cache = new Mock<IMemoryCache>();
            var myContext = new Mock<HttpContext>();
            FlightPlanController controller = new FlightPlanController(memoryCache, factory);

            // Act
            Task<ActionResult<FlightPlan>> flightPlan = controller.GetFlightPlanById("dfasd3");


            // Assert
            Assert.IsNull(flightPlan.Result.Value);
        }

        /// <summary>
        /// Lates the relative date time.
        /// </summary>
        [TestMethod]
        public void LateRelativeDateTime()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            Mock<IMemoryCache> cache = new Mock<IMemoryCache>();

            var myContext = new Mock<HttpContext>();

            DateTime now = DateTime.Now;
            FlightPlan flightPlan = new FlightPlan();
            flightPlan.passengers = 200;
            var json = JsonConvert.SerializeObject(flightPlan);
            var byteArray = Encoding.ASCII.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Flush();
            stream.Position = 0;

            myContext.SetupGet(x => x.Request.Body).Returns(stream);
            var controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            FlightPlanController controller = new FlightPlanController(memoryCache, factory)
            {
                ControllerContext = controllerContext,
            };

            // Act
            ActionResult<FlightPlan> answer = controller.AddFlightPlan(flightPlan);

            // Assert
            Assert.AreEqual(answer.Value, flightPlan);
        }

        /// <summary>
        /// Gets the flight plan.
        /// </summary>
        [TestMethod]
        public void GetFlightPlan()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var clientHandlerStub = new DelegatingHandlerStub();
            var client = new HttpClient(clientHandlerStub);
            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            IHttpClientFactory factory = mockFactory.Object;
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            Mock<IMemoryCache> cache = new Mock<IMemoryCache>();

            var myContext = new Mock<HttpContext>();

            DateTime now = DateTime.Now;
            FlightPlan flightPlan = new FlightPlan();

            flightPlan.company_name = "swiss";
            flightPlan.passengers = 200;
            flightPlan.initial_location = new InitialLocation();
            flightPlan.initial_location.date_time = DateTime.Now;
            flightPlan.segments = new List<Segments>();
            flightPlan.segments.Add(new Segments());
            flightPlan.segments[0].timespan_seconds = 100;

            var json = JsonConvert.SerializeObject(flightPlan);
            var byteArray = Encoding.ASCII.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Flush();
            stream.Position = 0;

            myContext.SetupGet(x => x.Request.Body).Returns(stream);
            var controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            FlightPlanController controller = new FlightPlanController(memoryCache, factory)
            {
                ControllerContext = controllerContext,
            };

            // Act
            controller.AddFlightPlan(flightPlan);

            // Arrange
            myContext = new Mock<HttpContext>();
            myContext.SetupGet(x => x.Request.QueryString).
                Returns(new QueryString("?relative_to=" + DateTime.Now));
            controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            controller = new FlightPlanController(memoryCache, factory)
            {
                ControllerContext = controllerContext,
            };

            //Act
            var list = controller.GetFlightsByDateAsync(DateTime.Now);

            // Assert
            Assert.IsTrue(list.Result.Value.Count == 1);

            // Act
            Task<ActionResult<FlightPlan>> answer = 
                controller.GetFlightPlanById(list.Result.Value[0].flight_id);

            // Assert
            Assert.AreEqual(answer.Result.Value, flightPlan);

            // Act
            controller.DeleteFlight(list.Result.Value[0].flight_id);
            list = controller.GetFlightsByDateAsync(DateTime.Now);

            // Assert
            Assert.IsTrue(list.Result.Value.Count == 0);
        }

        /// <summary>
        /// Checks the servers.
        /// </summary>
        [TestMethod]
        public void CheckServers()
        {
            // Arrange
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            Mock<IMemoryCache> cache = new Mock<IMemoryCache>();

            var myContext = new Mock<HttpContext>();

            DateTime now = DateTime.Now;
            Servers server = new Servers();

            server.ServerId = "das423";
            server.ServerURL = "/http/8080";

            var json = JsonConvert.SerializeObject(server);
            var byteArray = Encoding.ASCII.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            stream.Flush();
            stream.Position = 0;

            myContext.SetupGet(x => x.Request.Body).Returns(stream);
            var controllerContext = new ControllerContext()
            {
                HttpContext = myContext.Object,
            };
            ServersController controller = new ServersController(memoryCache)
            {
                ControllerContext = controllerContext,
            };

            // Act
            var answer = controller.AddServer(server);

            // Assert
            Assert.AreEqual(answer.Value, server);

            // Arrange
            controller = new ServersController(memoryCache);

            // Act
            ActionResult<List<Servers>> list = controller.GetServers();

            // Assert
            Assert.IsTrue(list.Value.Count == 1);

            // Act
            answer = controller.DeleteServerById(server.ServerId);

            // Assert
            Assert.AreEqual(answer.Value, server);
        }
    }
}
