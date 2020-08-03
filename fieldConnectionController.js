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
        $("#valueMother").append("<option>Any</option>");

        for (var i = 0; i < jsons.length; i++) {
                if (jsons[i].name == selectedOption) {
                    for (var j = 0; j < jsons[i].values.length; j++) {
                        var currValue = jsons[i].values[j];
                        $("#valueMother").append("<option>" + currValue + "</option>");
                    }
                }            
        }
    });

    $("#submit").click(function () {
        var selectedField = $("#fieldMother option:selected").text();
        var selectedValue = $("#valueMother option:selected").text();
        if (selectedField != "Any" && selectedField != "" && selectedValue != "Any" && selectedValue != "") {
            var childrenFields = document.getElementsByClassName("fieldChildren");
            for (var i = 0; i < childrenFields.length; i++) {
                if (childrenFields[i].checked) {
                    var connection = selectedField + ":" + selectedValue + "->" + childrenFields[i].id;
                    if ($("#connections").val() != "") {
                        var addedFields = $("#connections").val().split('\n');
                        var foundCoincidence = false;
                        for (var j = 0; j < addedFields.length; j++) {
                            if (addedFields[j].split("->")[0] == connection.split("->")[0]) {
                                if (addedFields[j].split("->")[1].includes(connection.split("->")[1].trim())) {
                                    foundCoincidence = true;
                                    break;
                                }
                                else {
                                    addedFields[j] = addedFields[j].split("->")[0] + "->" + addedFields[j].split("->")[1] + "," + childrenFields[i].id;
                                    foundCoincidence = true;
                                    break;
                                }
                            }
                        }
                        if (!foundCoincidence) {
                            addedFields.push(connection);
                        }
                        $("#connections").val(addedFields.join('\n'));
                    }
                    else {
                        $('#connections').val(connection);
                    }
                }
            }
        }
        
    });

    $("#valueMother").change(function () {
        //чекнуть все чекбоксы, которые законнекчены
        // добавить после реализации первого коннекта
    });
        
}

