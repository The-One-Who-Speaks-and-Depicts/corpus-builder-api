﻿jsons = [];

$(document).ready(function () {
    var values = document.getElementById('values');
    splitValues = values.innerText.split('|');
    for (var i = 0; i < splitValues.length; i++) {
        $.getJSON("/database/fields/" + splitValues[i], function (data) {
            if (data.type != "Document" && data.type != "Text") {
                jsons.push(data);
                $("#keys").append("<option>" + data.name + "</option>");
            }
        });
    }
    jsons.sort((a, b) => (a.name > b.name) ? 1 : ((b.name > a.name) ? -1 : 0));

    $("#type").change(function () {
        if ($("keys").css("opacity") == undefined) {
            $("#keys").css("opacity", 1);
        }
        $("#userValue").css("opacity", 0);
        $("#fieldValues").css("opacity", 0);
        $("#keys").text("");
        $("#keys").append("<option>Any</option>");
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].type == $("#type option:selected").text()) {
                $("#keys").append("<option>" + jsons[i].name + "</option>");
            }            
        }
    });
    
    $("#keys").change(function () {
        $("#fieldValues").text("");
        $("#userValue").val(""); 
        var selectedOption = $("#keys option:selected").text();
        
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].name == selectedOption) {
                if (!jsons[i].isUserFilled) {
                    $("#userValue").css("opacity", 0);
                    $("#fieldValues").css("opacity", 1);
                    $("#fieldValues").append("<option>Any</option>");
                    for (var j = 0; j < jsons[i].values.length; j++) {
                        var currValue = jsons[i].values[j];
                        $("#fieldValues").append("<option>" + currValue + "</option>");
                    }
                }
                else {
                    $("#userValue").css("opacity", 1);
                    $("#fieldValues").css("opacity", 0);
                }
            }            
        }
    });

    let signCounter = 0;

    $("#showFeature").click(function () {
        
        $('#features').append("<textarea rows=\"1\" cols=\"50\" id=\"text" + signCounter + "\"></textarea>  ");
        $("#features").append("<input type=\"color\" value=\"#FFFFFF\" id=\"signOfColor" + signCounter + "\" name=\"signOfColor" + signCounter + "\">");
        $("#features").append("<button onclick=\"Depict('text" + signCounter + "');\" id=\"text" + signCounter + "Button\">Отобразить признак</button>");
        $("#features").append("<br />");
        signCounter++;
        

        Depict = function (selectedOption) {
            optionColor = "#signOfColor" + selectedOption.split('text')[1];
            selectedOption = '#' + selectedOption;
            selectedOption = $(selectedOption).val().trim();
            selectedOption = selectedOption.split(' ');
            for (let i = 0; i < selectedOption.length; i++) {
                selectedOption[i] = selectedOption[i].split(':');
            }
            var words = document.getElementsByClassName('word');
            var clauses = document.getElementsByClassName('clause');
            var letters = document.getElementsByClassName('grapheme');
            var collection;
            var everythingCorrelates = true;
            var optionToPick = "";
            for (let i = 0; i < jsons.length; i++) {
                if (jsons[i].name == selectedOption[0][0]) {
                    optionToPick = jsons[i].type;
                }
                else if (jsons[i].name == selectedOption[0][0].substring(1)) {
                    optionToPick = jsons[i].type;
                }
            }
            optionToPick = optionToPick.trim();
            for (let i = 0; i < selectedOption.length; i++) {
                for (let j = 0; j < jsons.length; j++) {
                    if ((selectedOption[i][0] == jsons[j].name) & (optionToPick != jsons[j].type)) {
                        everythingCorrelates = false;
                    }
                }
                
            }
            if (everythingCorrelates) {
                if (optionToPick == "Clause") {
                    collection = clauses;
                }
                else if (optionToPick == "Realization") {
                    collection = words;
                }
                else {
                    collection = letters;
                }
                var contents = "";
                for (var i = 0; i < collection.length; i++) {
                    contents = $(collection[i]).attr('data-content');
                    if (contents.includes("***")) {
                        lexemeContents = contents.split("***");
                        for (let c = 0; c < lexemeContents.length; c++) {
                            if (lexemeContents[c] == "") {
                                continue;
                            }
                            contents = lexemeContents[c].split(";<br />");
                            for (var j = 0; j < contents.length; j++) {
                                contents[j] = contents[j].split(':');
                            }
                            coincidences = 0;
                            for (var j = 0; j < selectedOption.length; j++) {
                                coincides = false;
                                for (var k = 0; k < contents.length; k++) {
                                    if (contents[k] == "") {
                                        continue;
                                    }
                                    if (selectedOption[j][0][0] == '!') {
                                        var positive = selectedOption[j][0];
                                        positive = positive.slice(1);
                                        if (positive == contents[k][0]) {
                                            if (!contents[k][1].match(selectedOption[j][1])) {
                                                coincides = true;
                                            }
                                            else {
                                                coincides = false;
                                                break;
                                            }
                                        }
                                        else {
                                            coincides = true;
                                        }
                                    }
                                    else {
                                        if (selectedOption[j][0] == contents[k][0]) {
                                            if (contents[k][1].match(selectedOption[j][1])) {
                                                coincides = true;
                                                break;
                                            }
                                            else {
                                                coincides = false;
                                            }
                                        }
                                        else {
                                            coincides = false;
                                        }
                                    }
                                }
                                if (coincides) {
                                    coincidences++;
                                }
                            }
                            if (coincidences == selectedOption.length) {
                                var color = $(optionColor).val();
                                collection[i].style.backgroundColor = color;
                                if ($(collection[i]).children().length > 0) {
                                    $(collection[i]).children().css("background-color", color);
                                    for (let j = 0; j < $(collection[i]).children().length; j++) {
                                        if ($($(collection[i]).children()[j]).children().length > 0) {
                                            $($(collection[i]).children()[j]).children().css("background-color", color);
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else {
                        contents = contents.split(";<br />");
                        for (var j = 0; j < contents.length; j++) {
                            contents[j] = contents[j].split(':');
                        }
                        coincidences = 0;
                        for (var j = 0; j < selectedOption.length; j++) {
                            coincides = false;
                            for (var k = 0; k < contents.length; k++) {
                                if (selectedOption[j][0][0] == '!') {
                                    var positive = selectedOption[j][0];
                                    positive = positive.slice(1);
                                    if (positive == contents[k][0]) {
                                        if (!contents[k][1].match(selectedOption[j][1])) {
                                            coincides = true;
                                        }
                                        else {
                                            coincides = false;
                                            break;
                                        }
                                    }
                                    else {
                                        coincides = true;
                                    }
                                }
                                else {
                                    if (selectedOption[j][0] == contents[k][0]) {
                                        if (contents[k][1].match(selectedOption[j][1])) {
                                            coincides = true;
                                            break;
                                        }
                                        else {
                                            coincides = false;
                                        }
                                    }
                                    else {
                                        coincides = false;
                                    }
                                }
                            }
                            if (coincides) {
                                coincidences++;
                            }
                        }
                        if (coincidences == selectedOption.length) {
                            var color = $(optionColor).val();
                            collection[i].style.backgroundColor = color;
                            if ($(collection[i]).children().length > 0) {
                                $(collection[i]).children().css("background-color", color);
                                for (let j = 0; j < $(collection[i]).children().length; j++) {
                                    if ($($(collection[i]).children()[j]).children().length > 0) {
                                        $($(collection[i]).children()[j]).children().css("background-color", color);
                                    }
                                }
                            }

                        }
                    }                    
                }
            }
            



        };


           
        
    });

    $("#addFeature").click(function () {
        var selectedField = "";
        var selectedValue = "";
        var lastTextField = "#text" + (signCounter - 1);
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
        else if ($("#fieldValues").css("opacity") == 1) {
            selectedValue = $("#fieldValues option:selected").text();
        }
        
        $(lastTextField).append(isNegative + selectedField + ":" + selectedValue + " ");
    });

    

});



