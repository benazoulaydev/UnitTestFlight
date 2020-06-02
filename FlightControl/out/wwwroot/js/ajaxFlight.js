
//call function 4 time per sec (every 250 milisec)

(function loop() {
    get_flight_time();
    setTimeout(loop, 1000); //recurse
})();
function get_flight_time() {

    var today = new Date();
    if ((today.getMonth() + 1) < 10) { var mounth = "0" + (today.getMonth() + 1); } else { var mounth = (today.getMonth() + 1); }
    if (today.getDate() < 10) { var day = "0" + today.getDate(); } else { var day = today.getDate(); }
    if (today.getHours() < 10) { var hour = "0" + today.getHours(); } else { var hour = today.getHours(); }
    if (today.getMinutes() < 10) { var min = "0" + today.getMinutes(); } else { var min = today.getMinutes(); }
    if (today.getSeconds() < 10) { var sec = "0" + today.getSeconds(); } else { var sec = today.getSeconds(); }
    var time = today.getFullYear() + "-" + mounth + "-" + day + "T" + hour + ":" + min + ":" + sec;


    $.ajax({
        type: "GET",
        url: "/api/Flights?relative_to=" + time + "&sync_all", // Using our resources.json file to serve results
        dataType: 'json',
        success: function (result) {
            display_flights(result);
            display_planes_icons(result);
            // call function with parameter result
			console.log(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {

        }
    });
}

function get_flightplan(id) {
    $.ajax({
        type: "GET",
        url: "/api/FlightPlan/" + id, // Using our resources.json file to serve results
        dataType: 'json',
        success: function (result) {
            // call function with parameter result
            setFlightPlanBox(result, id);
            drawFlightPlanLine(result, id);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            let al = document.getElementById('alrt');
            al.innerHTML = '<b>Error </b>' + XMLHttpRequest.responseJSON.status + ", " + XMLHttpRequest.responseJSON.title;
            al.style.opacity = "1.0";
            setTimeout(function () { document.getElementById('alrt').style.opacity = "0.0"; }, 5000);
        }
    });
}

function post_flightplan(flightPlan) {
    $.ajax({
        type: "POST",
        url: "/api/FlightPlan", // Using our resources.json file to serve results
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: flightPlan,
        success: function (result) {
            // call function with parameter result
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            let al = document.getElementById('alrt');
            al.innerHTML = '<b>Error </b>' + XMLHttpRequest.responseJSON.status + ", " + XMLHttpRequest.responseJSON.title;
            al.style.opacity = "1.0";
            setTimeout(function () { document.getElementById('alrt').style.opacity = "0.0"; }, 5000);
        }
    });
}

function delete_flightplan(id) {
    $.ajax({
        type: "DELETE",
        url: "/api/Flights/" + id, // Using our resources.json file to serve results
        success: function (result) {
            // call function with no parameter
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            let al = document.getElementById('alrt');
            al.innerHTML = '<b>Error </b>' + XMLHttpRequest.responseJSON.status + ", " + XMLHttpRequest.responseJSON.title;
            al.style.opacity = "1.0";
            setTimeout(function () { document.getElementById('alrt').style.opacity = "0.0"; }, 5000);
        }
    });
}