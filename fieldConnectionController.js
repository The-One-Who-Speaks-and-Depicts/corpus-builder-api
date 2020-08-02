jsons = []
window.onload = function () {
    var values = document.getElementById('values');
    splitValues = values.innerText.split('|');
    for (var i = 0; i < splitValues.length; i++) {
        if (splitValues[i] != "Any" && splitValues[i] != "") {
            $.getJSON("/database/fields/" + splitValues[i] + ".json", function (data) {
                if (!data.isUserFilled) {
                    jsons.push(data);
                    $("#fieldMother").append("<option>" + data.name + "</option>");
                }
            });
        }        
    }

    $("#fieldMother").change(function () {
        $("#valueMother").text("");
        var selectedOption = $("#fieldMother option:selected").text();

        for (var i = 0; i < jsons.length; i++) {
                if (jsons[i].name == selectedOption) {
                    for (var j = 0; j < jsons[i].values.length; j++) {
                        var currValue = jsons[i].values[j];
                        $("#valueMother").append("<option>" + currValue + "</option>");
                    }
                }            
        }
    });
        
}

