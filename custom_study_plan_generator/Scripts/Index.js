﻿$(document).ready(function () {

    $('#createButton').click(function() {

        var data = $('#studentIdInput').val();
        data += ",create";

        $.ajax({
            url: "../Home/CheckStudentID",
            type: "POST",
            data: { data: data },
            success: function (data) {
                if (data == "true") {
                    $('input[name=formInput]').val("create");
                    $('#selectionForm').submit();
                }
                else {
                    if ($('#error3').is(':animated')) {
                        $('#error3').stop().animate({ opacity: '100' });
                    }
                    $('#error3').text(data);
                    $('#error3').show();
                    $('#error3').fadeOut(5000);
                }
            },
            error: function (data) {
            
            }
        });

    });

    $('#editButton').click(function () {

        var data = $('#studentIdInput').val();
        data += ",edit";

        $.ajax({
            url: "../Home/CheckStudentID",
            type: "POST",
            data: { data: data },
            success: function (data) {
                if (data == "true") {
                    $('input[name=formInput]').val("edit");
                    $('#selectionForm').submit();
                }
                else {
                    if ($('#error3').is(':animated')) {
                        $('#error3').stop().animate({opacity:'100'});
                    }
                    $('#error3').text(data);
                    $('#error3').show();
                    $('#error3').fadeOut(5000);
                }
            },
            error: function (data) {
          
            }
        });

    });

});