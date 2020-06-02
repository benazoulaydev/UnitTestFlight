var chosenID = null;
var prevDeleted = null;

function display_flights(flights) {
    tableHtml = "";
    document.getElementById("internal_flights").innerHTML = tableHtml;
    for (i = 0; i < flights.length; i++) {
        if (!flights[i].is_external) {
            var long = flights[i].longitude.toString().substring(0, 5);
            var lati = flights[i].latitude.toString().substring(0, 5);
            tableHtml += "<a id='" + flights[i].flight_id + "' href='#' onclick='chooseLine(this.id)' class='list-group-item list-group-item-action flex-column align-items-start'>" +
                "<div class='d-flex w-100 justify-content-between' >" +
                "<h5 class='mb-1'>" + flights[i].flight_id + "   " + flights[i].company_name + "</h5>" +
                "</div >" +
                "<small>(" + long + ", " + lati + ")</small>" +
                "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +
                "</a >" +
                "<label id='" + flights[i].flight_id + "' onclick='deleteLine(this.id)'>X</label>";
        }
    }
    tableHtml += "<div class='d-flex w-100 justify-content-between'><h5 class='mb-0 text-center'>Externals</h5></div>";

    for (i = 0; i < flights.length; i++) {
        if (flights[i].is_external) {
            var long = flights[i].longitude.toString().substring(0, 5);
            var lati = flights[i].latitude.toString().substring(0, 5);
            tableHtml += "<a id='" + flights[i].flight_id + "' href='#' onclick='chooseLine(this.id)' class='list-group-item list-group-item-action flex-column align-items-start'>" +
                "<div class='d-flex w-100 justify-content-between' >" +
                "<h5 class='mb-1'>" + flights[i].flight_id + "   " + flights[i].company_name + "</h5>" +
                "</div >" +
                "<small>(" + long + ", " + lati + ")</small>" +
                "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" +
                "</a >";
        }
    }
    document.getElementById("internal_flights").innerHTML = tableHtml;
    if (chosenID !== null) {
        console.log("kfir2" + chosenID);
        document.getElementById(chosenID).style.backgroundColor = "powderblue";
    }
}

function upload(ev) {
    var element = document.getElementById("upLoad");
    var jsonfile;
    if (element.files.length > 0) {
        jsonfile = element.files[0];
    }

    var text;

    var reader = new FileReader();
    reader.onload = (function (data) {
        return function (e) {
            text = JSON.parse(e.target.result);
            text = JSON.stringify(text);
            post_flightplan(text);
        }
    })(jsonfile);

    reader.readAsText(jsonfile);
}

function chooseLine(id) {
    if (chosenID !== null) {
        document.getElementById(chosenID).style.backgroundColor = "powderblue";
    }
    chosenID = id;
    get_flightplan(id);
}

function deleteLine(id) {
    if (prevDeleted === id) {
        return;
    }
    prevDeleted = id;
    if (id === chosenID) {
        chosenID = null;
    }
    delete_flightplan(id);
    delete_flight(id);

    document.getElementById("identifer").innerHTML = id + " has deleted";
    document.getElementById("startPosition").innerHTML = "";
    document.getElementById("endPosition").innerHTML = "";
}

function resetBox() {
    document.getElementById("identifer").innerHTML = "";
    document.getElementById("startPosition").innerHTML = "";
    document.getElementById("endPosition").innerHTML = "";
}

function setFlightPlanBox(flightPlan, id) {
    var long = flightPlan.initial_location.longitude.toString().substring(0, 5);
    var lati = flightPlan.initial_location.latitude.toString().substring(0, 5);

    document.getElementById("identifer").innerHTML = id + " for " + flightPlan.company_name;
    document.getElementById("startPosition").innerHTML = "Begining at " + flightPlan.initial_location.date_time + " in (" +
        long + ", " + lati + ")";
    var endTime = new Date(flightPlan.initial_location.date_time);
    for (i = 0; i < flightPlan.segments.length; i++) {
        endTime.setSeconds(endTime.getSeconds() + flightPlan.segments[i].timespan_seconds);
    }
    var endLongitude = flightPlan.segments[flightPlan.segments.length - 1].longitude.toString().substring(0, 5);
    var endLatitude = flightPlan.segments[flightPlan.segments.length - 1].latitude.toString().substring(0, 5);
    var endTimeFormat = endTime.getFullYear() + "-" + (endTime.getMonth() + 1) + "-" + endTime.getDate() + "T" + endTime.getHours() + ":" + endTime.getMinutes() + ":" + endTime.getSeconds();
    document.getElementById("endPosition").innerHTML = "Ending at " + endTimeFormat + " in (" + endLongitude + ", " + endLatitude + ")";
}