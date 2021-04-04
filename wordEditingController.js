jsons = [];

$(document).ready = function () {
    $(".word").dblclick(function () {
        $('#info').text("");
        $('#info').append("Lexeme:");
        $('#info').append(this.innerHTML);
        $('#info').append("<br /><br /> Features:<br />");
        $('#info').append($(this).attr('data-content'));
        $('#info').append("<br /><br /><br />");
        $("#identificator").text("");
        $("#identificator").append($(this).attr('id'));     
    });    

    
    var values = document.getElementById('values');
    splitValues = values.innerText.split('|');
    for (var i = 0; i < splitValues.length; i++) {
        $.getJSON("../database/fields/" + splitValues[i], function (data) {
            jsons.push(data);
            $("#keys").append("<option>" + data.name + "</option>");
            
        });
    }

    $("#keys").change(function () {
        $("#thisFieldValues").text("");
        var selectedOption = $("#keys option:selected").text();
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].name == selectedOption) {
                for (var j = 0; j < jsons[i].values.length; j++) {
                    var currValue = jsons[i].values[j];
                    $("#thisFieldValues").append("<option>" + currValue + "</option>");
                }
                $("#fieldInfo").append(jsons[i].isMultiple);
            }
        }
    });

    
}


