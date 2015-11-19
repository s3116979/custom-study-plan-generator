﻿$(document).ready(function () {
     
    /* Dynamic sizing of background divs and table height */
    var heightTable = $('#planTable').height();
    $('#textCP').css("height", heightTable * 1.9);
    $('#backerCP').css("height", heightTable * 2.1);

    var heightTopCell = $('#topCell').height();
    var heightp1 = $('#p1').height();

    $('#planTable').css("height", heightTopCell + (heightp1 * 4));

    var semesters = (numUnits / 4) | 0;

    if (semesters > 6) {
        var percentage = 96 / semesters;
        var percString = percentage + "%";
        $('.cell').css("width", percString);
        $('.planHeader').css("width", percString)
    }

    var count = 1;
    if (studentPlan != null) {
        studentPlan.forEach(function (entry) {

            var innerCellId = "#" + count;

            if (entry != "" && entry.exempt == false) {
                idCont = "#p" + count;
               
                $(idCont).append("<div id = '" + count + "' class = 'innerCell active' draggable = 'true' ondragstart = 'drag(event)' ondragend = 'dragend(event, this)'></div>");
                $(innerCellId).text(entry.name);

            }
            else if (entry != "" && entry.exempt == true) {
                idCont = "#p" + count;
                $(idCont).append("<div id = '" + count + "' class = 'innerCell active marked' draggable = 'true' ondragstart = 'drag(event)' ondragend = 'dragend(event, this)'></div>");
                $(innerCellId).text(entry.name);
            }
            count++
        });
    }

    /* Mark and unmark exempt units */
    $(".innerCell").click(function () {

        var id = $(this).attr('id');
        id = '#' + id;

        if ($(id).hasClass("marked")) {
            $(id).removeClass("marked");
        }
        else {
            $(id).addClass("marked");
        }
    });

    /* Send an ajax array of selected units to the Home/RemoveExemptions controller */
    $('#removeExemptions').click(function () {

        /* Reset any previous errors/messages. */
        $('#errors').hide();
        $('#error2').html("");

        var id;
        var array = [];
        var elements = document.getElementsByClassName("marked");
        for (var x = 0; x < elements.length; x++) {
            id = $(elements[x]).get(0).id;
            array[x] = id;
        }

        $.ajax({
            url: 'RemoveExemptions',
            type: "POST",
            data: { data: array },
            success: function (response) {
                // Success - Redirect to the Algorithm.
                location.href = 'Modify';
            },
            error: function (data) {
                // Exemption limit exceeded - display error.
                $('#error2').html("Exemption Limit exceeded: Please select less units.");
                $('#error2').show();
                $('#error2').delay(5000).fadeOut('slow');
            }
        });
    });

});

$(window).on('resize', function () {
    var heightTable = $('#planTable').height();
    $('#textCP').css("height", heightTable * 1.9);
    $('#backerCP').css("height", heightTable * 2.1);

    var heightTopCell = $('#topCell').height();
    var heightp1 = $('#p1').height();

    $('#planTable').css("height", heightTopCell + (heightp1 * 4));
});