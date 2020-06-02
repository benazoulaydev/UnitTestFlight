//-----------------------------------------------------------------------
// <copyright file="FlightPlanController.cs" company="CompanyName">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------
namespace FlightSimulatorWebApi.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using FlightSimulatorWebApi.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;

    /// <summary>
    /// the main controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route("api/")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        /// <summary>
        /// The mutex for flight id
        /// </summary>
        Mutex mut = new Mutex();

        /// <summary>
        /// The rand for the flight id
        /// </summary>
        Random rand = new Random();

        /// <summary>
        /// The main cache
        /// </summary>
        private IMemoryCache cache;

        /// <summary>
        /// The client
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlightPlanController"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="factory">The factory.</param>
        public FlightPlanController(IMemoryCache cache, IHttpClientFactory factory)
        {
            this.cache = cache;
            this.client = factory.CreateClient("api");
        }

        /// <summary>
        /// Adds a flight plan.
        /// </summary>
        /// <param name="info">The infos.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("FlightPlan")]
        public ActionResult<FlightPlan> AddFlightPlan(FlightPlan info)
        {
            string flightPlanID;
            Dictionary<string, FlightPlan> flightPlans;
            //check if no value exist.
            if (!cache.TryGetValue("FlightPlans", out flightPlans))
            {
                flightPlans = new Dictionary<string, FlightPlan>();
                cache.Set("FlightPlans", flightPlans);
            }
            //add anyway to cache and pik uniq id for it
            do
            {
                flightPlanID = GetFlightID();
            } while (flightPlans.ContainsKey(flightPlanID));
            flightPlans.Add(flightPlanID, info);
            return Ok(info);
        }

        /// <summary>
        /// Gets the flight plan by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("FlightPlan/{id}")]
        public async Task<ActionResult<FlightPlan>> GetFlightPlanById(string id)
        {
            Dictionary<string, FlightPlan> flightPlans;
            // check if there any flightPlan has added ever
            if (!cache.TryGetValue("FlightPlans", out flightPlans))
            {
                Task<ActionResult<FlightPlan>> tmp = ServerIdFlightPlan(id);
                try
                {
                    return await tmp;
                }
                catch (Exception)
                {
                    return NotFound();
                }

            }
            FlightPlan flightPlan;
            // extract the FlightPlan object which asked for
            if (!flightPlans.TryGetValue(id, out flightPlan))
            {
                Task<ActionResult<FlightPlan>> tmp = ServerIdFlightPlan(id);
                try
                {
                    return await tmp;
                }
                catch (Exception)
                {
                    return NotFound();
                }

            }
            return Ok(flightPlan); ;
        }

        /// <summary>
        /// Servers the identifier flight plan.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public async Task<ActionResult<FlightPlan>> ServerIdFlightPlan(string id)
        {
            Dictionary<string, Servers> servers;
            if (cache.TryGetValue("servers", out servers))
            {
                foreach (KeyValuePair<string, Servers> server in servers)
                {
                    HttpResponseMessage response = await client.GetAsync(server.Value.ServerURL +
                        "/api/FlightPlan/" + id);
                    response.EnsureSuccessStatusCode();
                    var resp = await response.Content.ReadAsStringAsync();

                    FlightPlan serverflightPlan = JsonConvert.DeserializeObject<FlightPlan>(resp);

                    return Ok(serverflightPlan);



                }
            }
            else
            {
                return NotFound();
            }
            return NotFound();
        }

        /// <summary>
        /// Adds to flight list if it is in the relative time.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="flightList">The flight list.</param>
        /// <param name="relativeTo">The relative to.</param>
        private void AddToFlightListIfShould(KeyValuePair<string, FlightPlan> entry,
            List<Flight> flightList, DateTime relativeTo)
        {
            DateTime entryKeyTimeAfter = entry.Value.initial_location.date_time;
            DateTime entryKeyTimeBefore = entry.Value.initial_location.date_time;
            if (entry.Value.initial_location.date_time > relativeTo)
            {
                return;
            }
            double latitudeBefore = entry.Value.initial_location.latitude;
            double longitudeBefore = entry.Value.initial_location.longitude;


            foreach (var segment in entry.Value.segments)
            {

                double latitudeAfter = segment.latitude;
                double longitudeAfter = segment.longitude;

                entryKeyTimeAfter = entryKeyTimeAfter.AddSeconds(segment.timespan_seconds);

                if (entryKeyTimeBefore <= relativeTo && entryKeyTimeAfter >= relativeTo)
                {
                    Flight tmp = new Flight();
                    tmp.flight_id = entry.Key.ToString();
                    tmp.passengers = entry.Value.passengers;
                    tmp.company_name = entry.Value.company_name;
                    tmp.date_time = entry.Value.initial_location.date_time;

                    double prop = propFinder(entryKeyTimeBefore, entryKeyTimeAfter, relativeTo);

                    tmp.longitude = LongituteFinder(longitudeBefore, longitudeAfter, prop);
                    tmp.latitude = LatitudeFinder(latitudeBefore, latitudeAfter, prop);
                    tmp.is_external = false;

                    flightList.Add(tmp);
                    break;
                }
                entryKeyTimeBefore.AddSeconds(segment.timespan_seconds);

                latitudeBefore = segment.latitude;
                longitudeBefore = segment.longitude;
            }
        }

        /// <summary>
        /// Changes to external.
        /// </summary>
        /// <param name="serverFlights">The server flights.</param>
        private void ChangeToExternal(List<Flight> serverFlights)
        {
            foreach (Flight flight in serverFlights)
            {
                flight.is_external = true;
            }
        }

        /// <summary>
        /// Gets the flights by relative to date asynchronous.
        /// </summary>
        /// <param name="relative_to">The relative to.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Flights")]
        public async Task<ActionResult<List<Flight>>> GetFlightsByDateAsync(
            [FromQuery(Name = "relative_to")] DateTime relativeTo)
        {

            List<Flight> flightList = new List<Flight>();
            Dictionary<string, FlightPlan> flightPlans;
            // check if there any flight at all
            if (!cache.TryGetValue("FlightPlans", out flightPlans))
            {
                flightPlans = new Dictionary<string, FlightPlan>();
                cache.Set("FlightPlans", flightPlans);
            }
            // going throw each flight
            foreach (KeyValuePair<string, FlightPlan> entry in flightPlans)
            {
                AddToFlightListIfShould(entry, flightList, relativeTo);
            }



            //check from other servers
            Dictionary<string, Servers> servers;
            if (!Request.QueryString.Value.Contains("sync_all") ||
                !cache.TryGetValue("servers", out servers))
            {
                return Ok(flightList);
            }
            foreach (KeyValuePair<string, Servers> server in servers)
            {
                try
                {
                    // async query other servers their flights
                    HttpResponseMessage response = await client.GetAsync(server.Value.ServerURL +
                        "/api/Flights?relative_to=" + relativeTo.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    response.EnsureSuccessStatusCode();
                    var resp = await response.Content.ReadAsStringAsync();
                    List<Flight> serverFlights = JsonConvert.DeserializeObject<List<Flight>>(resp);
                    ChangeToExternal(serverFlights);
                    //merg lists
                    flightList = flightList.Concat(serverFlights).ToList();
                }
                catch (NullReferenceException)
                {
                    break;
                }
            }
            return Ok(flightList);
        }


        /// <summary>
        /// Deletes the flight.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Flights/{id}")]
        public ActionResult<FlightPlan> DeleteFlight(string id)
        {
            Dictionary<string, FlightPlan> flightPlans;
            if (!cache.TryGetValue("FlightPlans", out flightPlans))
            {
                return NotFound();
            }
            FlightPlan flightPlan;
            if (!flightPlans.TryGetValue(id, out flightPlan))
            {
                return NotFound();
            }
            flightPlans.Remove(id);
            return Ok(id);
        }

        /// <summary>
        /// Properties the finder.
        /// </summary>
        /// <param name="TimeBefore">The time before.</param>
        /// <param name="TimeAfter">The time after.</param>
        /// <param name="Between">The between.</param>
        /// <returns></returns>
        public double propFinder(DateTime TimeBefore, DateTime TimeAfter, DateTime Between)
        {
            double tmp = (Between - TimeBefore) / (TimeAfter - TimeBefore);
            return tmp;
        }

        /// <summary>
        /// Longitutes the finder.
        /// </summary>
        /// <param name="firstY">The first y.</param>
        /// <param name="secondY">The second y.</param>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        public double LongituteFinder(double firstY, double secondY, double prop)
        {
            double dy = secondY - firstY;
            return firstY + (dy * prop);
        }

        /// <summary>
        /// Latitudes the finder.
        /// </summary>
        /// <param name="firstX">The first x.</param>
        /// <param name="secondX">The second x.</param>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        public double LatitudeFinder(double firstX, double secondX, double prop)
        {
            double dx = secondX - firstX;
            return firstX + (dx * prop);
        }

        /// <summary>
        /// Gets the flight identifier.
        /// </summary>
        /// <returns></returns>
        private string GetFlightID()
        {
            string bigLs;
            string smallLs;
            string digs;
            string newCode;

            mut.WaitOne();

            bigLs = GetBigLetters(rand.Next(2, 3));
            smallLs = GetSmallLetters(rand.Next(2, 3));
            digs = GetDigits(rand.Next(2, 3));

            newCode = (bigLs + smallLs + digs);

            mut.ReleaseMutex();
            return newCode;
        }

        /// <summary>
        /// Gets the big letters.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private string GetBigLetters(int n)
        {
            int codeN;
            char codeC;
            string result = "";
            for (int i = 0; i < n; i++)
            {
                codeN = rand.Next(65, 91);
                codeC = (char)codeN;
                result += codeC;
            }
            return result;
        }

        /// <summary>
        /// Gets the small letters.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private string GetSmallLetters(int n)
        {
            int codeN;
            char codeC;
            string result = "";
            for (int i = 0; i < n; i++)
            {
                codeN = rand.Next(97, 123);
                codeC = (char)codeN;
                result += codeC;
            }
            return result;
        }

        /// <summary>
        /// Gets the digits.
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private string GetDigits(int n)
        {
            int codeN;
            string result = "";
            for (int i = 0; i < n; i++)
            {
                codeN = rand.Next(0, 10);
                result += codeN.ToString();
            }
            return result;
        }
    }
}