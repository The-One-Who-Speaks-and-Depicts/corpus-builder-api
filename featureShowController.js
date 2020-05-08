jsons = [];

window.onload = function () {
    var values = document.getElementById('values');
    splitValues = values.innerText.split('|');
    for (var i = 0; i < splitValues.length; i++) {
        $.getJSON("/database/fields/" + splitValues[i], function (data) {
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
    $("#showFeature").click(function () {
        var selectedOption = $("#keys option:selected").text();
        $("#features").append("<div id=\"" + selectedOption + "\">");
        $("#features").append(selectedOption + ":<br />");
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].name == selectedOption) {
                for (var j = 0; j < jsons[i].values.length; j++) {
                    var currValue = jsons[i].values[j];
                    $("#features").append(currValue + " ");
                    $("#features").append("<input type=\"color\" value=\"#FFFFFF\" name=\"" + selectedOption + "_" + currValue + "\">");
                    $("#features").append("<br />");
                }
            }
        }
        $("#features").append("<button onclick=\"Depict('"+ selectedOption + "');\" id=\"" + selectedOption + "\">Отобразить признак</button>");
        $("#features").append("</div>");
    });
};

function Depict(selectedOption) {
    var words = document.getElementsByClassName('word');
    var contents = "";
    for (var i = 0; i < words.length; i++) {        
        contents = $(words[i]).attr('data-content');
        contents = contents.split(':');
        if (contents[0] == selectedOption) {
            wordFeatures = contents[1].split(";");
            for (var j = 0; j < wordFeatures.length; j++) {
                if (wordFeatures[j] != "<br />") {
                    var features = document.getElementsByTagName("input");
                    console.log(features.length);
                    for (var k = 1; k < features.length; k++) {
                        console.log($(features[k]).attr('name').split('_')[1]);
                        console.log(wordFeatures[j]);
                        if ($(features[k]).attr('name').split('_')[1] == wordFeatures[j]) {
                            var color = $(features[k]).val();
                            words[i].style.backgroundColor = color;
                        }
                    }
                }
            } 
        }               
    }

    

};

