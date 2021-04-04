jsons = [];
$(document).ready(function () {
    var values = document.getElementById('values');
    splitValues = values.innerText.split('|');
    for (var i = 0; i < splitValues.length; i++) {
        $.getJSON("/database/fields/" + splitValues[i], function (data) {
            jsons.push(data);
            $("#keys").append("<option>" + data.name + "</option>");
        });
    }
    jsons.sort((a, b) => (a.name > b.name) ? 1 : ((b.name > a.name) ? -1 : 0));

    $("#keys").change(function () {
        $("#thisFieldValues").text("");
        $("#userValue").val("");
        var selectedOption = $("#keys option:selected").text();

        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].name == selectedOption) {
                if (!jsons[i].isUserFilled) {
                    $("#userValue").css("opacity", 0);
                    $("#thisFieldValues").css("opacity", 1);
                    $("#thisFieldValues").append("<option>Any</option>");
                    for (var j = 0; j < jsons[i].values.length; j++) {
                        var currValue = jsons[i].values[j];
                        $("#thisFieldValues").append("<option>" + currValue + "</option>");
                    }
                }
                else {
                    $("#userValue").css("opacity", 1);
                    $("#thisFieldValues").css("opacity", 0);
                }
            }
        }
    });

    $("#addFeature").click(function () {
        var selectedField = "";
        var selectedValue = "";
        isNegative = "";
        if ($('#isNegative').is(':checked')) {
            isNegative = "!";
        }
        else {
            isNegative = "";
        }
        selectedField = $("#keys option:selected").text();
        if ($("#userValue").css("opacity") == 1) {
            selectedValue = $("#userValue").val();
        }
        else if ($("#thisFieldValues").css("opacity") == 1) {
            selectedValue = $("#thisFieldValues option:selected").text();
        }

        $("#feature").append(isNegative + selectedField + ":" + selectedValue + "||");
    });

});
