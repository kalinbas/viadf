var map;
var marker;

function openMap(hiddenField) {
            
    if (map === undefined) {   
        var position = new google.maps.LatLng();
        var myOptions = {                     
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            streetViewControl: false,
            mapTypeControl: false,
        };

        map = new google.maps.Map(document.getElementById("modalmap"), myOptions);          
        marker = new google.maps.Marker({
            position: position,
            map: map,
            draggable: true                
        });            
    }

    google.maps.event.clearListeners(marker, 'dragend');
    google.maps.event.addListener(marker, "dragend", function (event) {
        var point = marker.getPosition();
        map.panTo(point);
        $("#" + hiddenField).val(point.lat() + "," + point.lng());
    });

    var coords = $("#"+hiddenField).val();
    if (coords.length > 0) {
        setMapToPosition(coords, 16, map, marker);
    } else {
        setMapToPosition("19.432681272968093,-99.13328689947508", 11, map, marker);
    }

    $("#dialog-modal").dialog({
        modal: true,
        closeText: 'x'
    });

    $(".ui-widget-overlay").click (function () {
        $("#dialog-modal").dialog( "close" );
    });
}

function setMapToPosition(input, zoom, map, marker) {
    var latlngStr = input.split(",", 2);
    var lat = parseFloat(latlngStr[0]);
    var lng = parseFloat(latlngStr[1]);
    var position = new google.maps.LatLng(lat, lng);
    map.setCenter(position);
    map.setZoom(zoom);
    marker.setPosition(position);
}

function isNumber(n) {
  return !isNaN(parseFloat(n)) && isFinite(n);
}
