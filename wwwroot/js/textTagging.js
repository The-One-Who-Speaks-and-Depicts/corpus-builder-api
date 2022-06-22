jsons = [];

$(document).ready(function () {



    var values = document.getElementById('values');
    splitValues = values.innerText.split('|');
    for (var i = 0; i < splitValues.length; i++) {
        $.getJSON("/database/fields/" + splitValues[i] + ".json", function (data) {
            jsons.push(data);
            if (data.type == "Section") {
                $("#keys").append("<option>" + data.name + "</option>");
            }

        });
    }

    $("#keys").change(function () {
        $("#thisFieldValues").text("");
        $("#userValue").text("");
        $("#connected").text("");
        var selectedOption = $("#keys option:selected").text();
        if (selectedOption == "Any") {
            if ($("#thisFieldValues").css('opacity') == 1) {
                $("#thisFieldValues").css('opacity', '0.0');
            }
            if ($("#userValue").css('opacity') == 1) {
                $("#userValue").css('opacity', '0.0');
            }
        }
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].name == selectedOption) {
                $("#userValue").css('opacity', '0.0');
                $("#thisFieldValues").css('opacity', '1.0');
                if (jsons[i].isUserFilled == false) {
                    $("#thisFieldValues").append("<option>Any</option>");
                    for (var j = 0; j < jsons[i].values.length; j++) {
                        var currValue = jsons[i].values[j];
                        $("#thisFieldValues").append("<option>" + currValue + "</option>");
                    }
                }
                else {
                    $("#thisFieldValues").css('opacity', '0.0');
                    $("#userValue").css('opacity', '1.0');
                }
            }
        }
    });

    $("#thisFieldValues").change(function () {
        var selectedField = $("#keys option:selected").text();
        var selectedValue = $("#thisFieldValues option:selected").text();
        $("#connected").text("");
        if (selectedValue != "Any") {
            for (var i = 0; i < jsons.length; i++) {
                if (jsons[i].name == selectedField) {
                    for (var j = 0; j < jsons[i].values.length; j++) {
                        if (jsons[i].values[j] == selectedValue) {
                            if (Object.keys(jsons[i].connectedFields).length > 0 && selectedValue in jsons[i].connectedFields) {
                                $('#connected').append("<select id=\"connectedFields\"><option>Any</option></select><br><br>")
                                for (var k = 0; k < Object.keys(jsons[i].connectedFields).length; k++) {
                                    if (Object.keys(jsons[i].connectedFields)[k] == selectedValue) {
                                        for (var l = 0; l < jsons[i].connectedFields[Object.keys(jsons[i].connectedFields)[k]].length; l++) {
                                            $("#connectedFields").append("<option>" + jsons[i].connectedFields[Object.keys(jsons[i].connectedFields)[k]][l] + "</option>");
                                            $("#connected").append("<select id=\"connectedRestrictedValue\" style=\"opacity:0.0\"><option>Any</option></select><br><br>");
                                            $("#connected").append("<textarea id=\"connectedUserValue\" style=\"opacity:0.0\"></textarea><br><br>");
                                            $('#connected').append("<button id=\"additionalFeatureButton\" type=\"button\">Добавить связанный признак в разметку</button><br />")
                                        }
                                    }
                                }
                                $("#connectedFields").change(function () {
                                    $("#connectedRestrictedValue").text("");
                                    $("#connectedUserValue").text("");
                                    $("#connectedRestrictedValue").css('opacity', '0.0');
                                    $("#connectedUserValue").css('opacity', '0.0');
                                    var selectedOption = $("#connectedFields option:selected").text();
                                    for (var i = 0; i < jsons.length; i++) {
                                        if (jsons[i].name == selectedOption) {
                                            $("#connectedUserValue").css('opacity', '0.0');
                                            $("#connectedRestrictedValue").css('opacity', '1.0');
                                            if (jsons[i].isUserFilled == false) {
                                                $("#connectedRestrictedValue").append("<option>Any</option>");
                                                for (var j = 0; j < jsons[i].values.length; j++) {
                                                    var currValue = jsons[i].values[j];
                                                    $("#connectedRestrictedValue").append("<option>" + currValue + "</option>");
                                                }
                                            }
                                            else {
                                                $("#connectedRestrictedValue").css('opacity', '0.0');
                                                $("#connectedUserValue").css('opacity', '1.0');
                                            }
                                        }
                                    }
                                });

                                $("#additionalFeatureButton").click(function () {
                                    var currentFields = $("#connections").val().split('\n');
                                    if ($("#connectedRestrictedValue").css("opacity") > 0 && $("#connectedRestrictedValue option:selected").text() != "Any") {
                                        if ($("#connections").val() != "") {
                                            var coincidenceFound = false;
                                            for (var i = 0; i < currentFields.length; i++) {
                                                if (currentFields[i].split('=>')[0] == $("#connectedFields option:selected").text()) {
                                                    for (var j = 0; j < currentFields[i].split('=>')[1].split(';').length; j++) {
                                                        if (currentFields[i].split('=>')[1].split(';')[j] == $("#connectedRestrictedValue option:selected").text()) {
                                                            coincidenceFound = true;
                                                            break;
                                                        }
                                                    }
                                                    for (var k = 0; k < jsons.length; k++) {
                                                        if ($("#connectedFields option:selected").text() == jsons[k].name) {
                                                            if (!jsons[k].isMultiple) {
                                                                coincidenceFound = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    if (!coincidenceFound) {
                                                        currentFields[i] += $("#connectedRestrictedValue option:selected").text() + ";";
                                                        coincidenceFound = true;
                                                        break;
                                                    }

                                                }
                                            }
                                            if (!coincidenceFound) {
                                                var addedKeyValuePair = $("#connectedFields option:selected").text() + "=>" + $("#connectedRestrictedValue option:selected").text() + ";\n";
                                                $("#connections").val(currentFields.join('\n') + addedKeyValuePair);
                                            }
                                            else {
                                                $("#connections").val(currentFields.join('\n'));
                                            }

                                        }
                                        else {
                                            $("#connections").val($("#connectedFields option:selected").text() + "=>" + $("#connectedRestrictedValue option:selected").text() + ";\n");
                                        }
                                    }
                                    else if ($("#connectedUserValue").css("opacity") > 0 && $("#connectedUserValue").val() != "") {
                                        if ($("#connections").val() != "") {
                                            var coincidenceFound = false;
                                            for (var i = 0; i < currentFields.length; i++) {
                                                if (currentFields[i].split('=>')[0] == $("#connectedFields option:selected").text()) {
                                                    for (var j = 0; j < currentFields[i].split('=>')[1].split(';').length; j++) {
                                                        if (currentFields[i].split('=>')[1].split(';')[j] == $("#connectedUserValue").val()) {
                                                            coincidenceFound = true;
                                                            break;
                                                        }
                                                    }
                                                    for (var k = 0; k < jsons.length; k++) {
                                                        if ($("#connectedFields option:selected").text() == jsons[k].name) {
                                                            if (!jsons[k].isMultiple) {
                                                                coincidenceFound = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    if (!coincidenceFound) {
                                                        currentFields[i] += $("#connectedUserValue").val() + ";";
                                                        coincidenceFound = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!coincidenceFound) {
                                                var addedKeyValuePair = $("#connectedFields option:selected").text() + "=>" + $("#connectedUserValue").val() + ";\n"
                                                $("#connections").val(currentFields.join('\n') + addedKeyValuePair);
                                            }
                                            else {
                                                $("#connections").val(currentFields.join('\n'));
                                            }
                                        }
                                        else {
                                            $("#connections").val($("#connectedFields option:selected").text() + "=>" + $("#connectedUserValue").val() + ";\n");
                                        }
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }
    });


    $("#submit").click(function () {
        var currentFields = $("#connections").val().split('\n');
        if ($("#thisFieldValues").css("opacity") > 0 && $("#thisFieldValues option:selected").text() != "Any") {
            if ($("#connections").val() != "") {
                var coincidenceFound = false;
                for (var i = 0; i < currentFields.length; i++) {
                    if (currentFields[i].split('=>')[0] == $("#keys option:selected").text()) {
                        for (var j = 0; j < currentFields[i].split('=>')[1].split(';').length; j++) {
                            if (currentFields[i].split('=>')[1].split(';')[j] == $("#thisFieldValues option:selected").text()) {
                                coincidenceFound = true;
                                break;
                            }
                        }
                        for (var k = 0; k < jsons.length; k++) {
                            if ($("#keys option:selected").text() == jsons[k].name) {
                                if (!jsons[k].isMultiple) {
                                    coincidenceFound = true;
                                    break;
                                }
                            }
                        }
                        if (!coincidenceFound) {
                            currentFields[i] += $("#thisFieldValues option:selected").text() + ";";
                            coincidenceFound = true;
                            break;
                        }

                    }
                }
                if (!coincidenceFound) {
                    var addedKeyValuePair = $("#keys option:selected").text() + "=>" + $("#thisFieldValues option:selected").text() + ";\n";
                    $("#connections").val(currentFields.join('\n') + addedKeyValuePair);
                }
                else {
                    $("#connections").val(currentFields.join('\n'));
                }

            }
            else {
                $("#connections").val($("#keys option:selected").text() + "=>" + $("#thisFieldValues option:selected").text() + ";\n");
            }
        }
        else if ($("#userValue").css("opacity") > 0 && $("#userValue").val() != "") {
            if ($("#connections").val() != "") {
                var coincidenceFound = false;
                for (var i = 0; i < currentFields.length; i++) {
                    if (currentFields[i].split('=>')[0] == $("#keys option:selected").text()) {
                        for (var j = 0; j < currentFields[i].split('=>')[1].split(';').length; j++) {
                            if (currentFields[i].split('=>')[1].split(';')[j] == $("#userValue").val()) {
                                coincidenceFound = true;
                                break;
                            }
                        }
                        for (var k = 0; k < jsons.length; k++) {
                            if ($("#keys option:selected").text() == jsons[k].name) {
                                if (!jsons[k].isMultiple) {
                                    coincidenceFound = true;
                                    break;
                                }
                            }
                        }
                        if (!coincidenceFound) {
                            currentFields[i] += $("#userValue").val() + ";";
                            coincidenceFound = true;
                            break;
                        }
                    }
                }
                if (!coincidenceFound) {
                    var addedKeyValuePair = $("#keys option:selected").text() + "=>" + $("#userValue").val() + ";\n"
                    $("#connections").val(currentFields.join('\n') + addedKeyValuePair);
                }
                else {
                    $("#connections").val(currentFields.join('\n'));
                }
            }
            else {
                $("#connections").val($("#keys option:selected").text() + "=>" + $("#userValue").val() + ";\n");
            }
        }
    });

    $("#clear").click(function () {
        $("#connections").val("");
        $("#clear").blur();
    });

});
