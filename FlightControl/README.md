# Flight Simulator Web Api

Created by [KfirYehuda](https://github.com/kfiryehuda), [stavih19](https://github.com/stavih19) and
 [BenAzoulayDev](https://github.com/benazoulaydev)

Link to [Repository](https://github.com/benazoulaydev/flightSimulatorWebApi/)

In this Project we will build an airplane control system, which is required to monitor active flights and allows to enter
News Flight. The flight plan describes the take-off time of the aircraft from a particular point on Earth as well
Consists of a list of waypoints where the plane passes and the estimated time it reaches each point. During the exercise
Assume the flights are running as planned and the air traffic control system advertises its location at any given time according to plan
Fed to him. The built airplane control system will be synchronized with other air control systems so that
users can also track the status of flights entered into their system and other flights entered into Exterior control systems.

## Use:

The built in app is a Web app, all user actions can be done from a single page in the browser. The display
And the user interface logic is  implemented dynamically on the client side (ViewModel, View), while all functionality
Required (Model) can be accessed by the client through resources (REST) that the server will expose via HTTP protocol

## Objectives:

1) Using Core NET.ASP to create a modern Web application.
2) Creating REST-based API using WebAPI
3) Client design is based on bootstrap design directories and css principles
4) Creating an ES8-based client-side app
5) Unit tests and Dependency Injection


## Api of the server:

| Action | Path | Description |
| ------ | ------ | ------ |
| GET | /api/Flights?relative_to=<DATE_TIME> | Returns an array in the body containing the states of all flights The activity entered directly to the current server (is_external = false) relative to the time specified in the request.| 
| GET | /api/Flights?relative_to=<DATE_TIME>&sync_all | Like the previous one, only that returns all the flights the server can Find out, both directly from him and from external servants Which is in sync with them.| 
| POST  | /api/FlightPlan | Feeds to serve a new flight plan, the flight plan Detailed by an appropriate object in the body of the request.| 
| GET | api/FlightPlan/{id} |  Returns the flight plan with the particular ID | 
| DELETE | /api/Flights/{id} | Deletes a flight with a specific ID previously entered to the server Current.| 
| GET | /api/servers | Returns the list of external servers you have served, Syncing information from them| 
| POST | /api/servers | Take an external server Listen from internal flight of the external server and display the external server flight in external flights| 
| DELETE | /api/servers/{id} | Deletes a server from the list of external servers| 

## Class Objects:

### Flights Json Format

```sh
{
 "flight_id": "[FLIGHT_ID]",
 "longitude": 33.244,
 "latitude": 31.12,
 "passengers": 216,
 "company_name": "SwissAir",
 "date_time": "2020-12-26T23:56:21Z",
 "is_external": false
}
```
### Flight plan Json Format

```sh
{
 "passengers": 216,
 "company_name": "SwissAir",
 "initial_location": {
 "longitude": 33.244,
 "latitude": 31.12,
 "date_time": "2020-12-26T23:56:21Z"
 },
 "segments": [
 {
 "longitude": 33.234,
 "latitude": 31.18,
 "timespan_seconds": 650
 },
 /*... more segments...*/
 ]
}
```

### Server Json Format

```sh
 "ServerId": "[SERVER_ID]",
 "ServerURL": "www.server.com"
```

## Deployment:

The application  run in an environment where 10 windows is installed with the latest version of 4 NET (version 6.4
And above) The compilation works on 2019 studio visual.
ASP.NET Core 3.1 WebAPI

## Resources:

Will be Added in the future...


## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)
