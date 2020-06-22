jsons = [];

window.onload = function () {


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
        $.getJSON("/database/fields/" + splitValues[i], function (data) {
            jsons.push(data);
            $("#keys").append("<option>" + data.name + "</option>");
            $("#keysFilter").append("<option>" + data.name + "</option>");

        });
    }

    $("#keys").change(function () {
        $("#thisFieldValues").text("");
		$("#fieldInfo").text("");
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
        $("#features").append("<button onclick=\"Depict('" + selectedOption + "');\" id=\"" + selectedOption + "\">Отобразить признак</button>");
        $("#features").append("</div>");
    });

    $("#keysFilter").change(function () {
        $("#valuesFilter").text("");
        var selectedOption = $("#keysFilter option:selected").text();
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].name == selectedOption) {
                for (var j = 0; j < jsons[i].values.length; j++) {
                    var currValue = jsons[i].values[j];
                    $("#valuesFilter").append("<option>" + currValue + "</option>");
                }
            }
        }
    });

    $("#featureAdditionButton").click(function () {
        var selectedOption = $("#keysFilter option:selected").text();
        isNegative = "";
        if ($("#isNegative").is(':checked'))
        {
          isNegative = "!";
        }
        $("#featureFilter").append(isNegative + selectedOption + ":");
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].name == selectedOption) {
                for (var j = 0; j < jsons[i].values.length; j++) {
                    var currValue = jsons[i].values[j];
                    var selectedValue = $("#valuesFilter option:selected").text();
                    if (currValue == selectedValue) {
                        $("#featureFilter").append(currValue + " ");
                    }
                }
            }
        }
    });

    $('#buttonFilter').click(function () {
        var selectedWord = document.getElementById('wordFilter').value;
        var selectedOptions = $('#featureFilter').val();
        if (selectedWord.length > 0 | selectedOptions.length > 0) {
            var words = document.getElementsByClassName('word');
            for (var i = 0; i < words.length; i++) {
                console.log('nw');
                if (selectedWord.length > 0) {
                    if (!isMatch(selectedWord, words[i].innerText)) {
                        words[i].setAttribute("style", "display:none;");
                    }
                }
                if (selectedOptions.length > 0) {
                    var optionsList = selectedOptions.split(' ');
                    var extantList = [];
                    for (var k = 0; k < optionsList.length; k++) {
                        if (optionsList[k].length > 0) {
                            extantList.push(optionsList[k]);
                        }
                    }
                    for (var k = 0; k < extantList.length; k++) {
                        extantList[k] = extantList[k].split(":");
                    }
                    var wordAttributes = words[i].getAttribute("data-content");
                    //if (wordAttributes.length > 0) {
                    wordAttributes = wordAttributes.split(";<br />");
                        var extantAttributes = [];
                        for (var k = 0; k < wordAttributes.length; k++) {
                            if (wordAttributes[k].length > 0) {
                                extantAttributes.push(wordAttributes[k]);
                            }
                        }
                        for (var k = 0; k < extantAttributes.length; k++) {
                            extantAttributes[k] = extantAttributes[k].split(":");
                        }
                        coincidingAttributes = 0;
                        for (var j = 0; j < extantList.length; j++) {
                            coincidenceFound = false;
                            if (extantList[j][0][0] == '!') {
                                if (extantAttributes.length < 1) {
                                    coincidenceFound = true;
                                }
                            }
                            exact = false;
                            for (var m = 0; m < extantAttributes.length; m++) {

                                if (extantList[j][0][0] != '!') {
                                    if (extantList[j][0] == extantAttributes[m][0]) {
                                        if (extantAttributes[m][1].match(extantList[j][1])) {
                                            coincidenceFound = true;
                                        }
                                    }
                                }
                                else {
                                    var positive = extantList[j][0].slice(1);
                                    if (positive == extantAttributes[m][0]) {
                                        if (extantAttributes[m][1].match(extantList[j][1])) {
                                            exact = true;
                                        }
                                        else {
                                            coincidenceFound = true;
                                        }
                                    }
                                    else {
                                        coincidenceFound = true;
                                    }
                                }

                            }
                            if (exact) {
                                coincidenceFound = false;
                            }
                            if (coincidenceFound) {
                                coincidingAttributes++;
                            }
                        }
                    if (coincidingAttributes == extantList.length) {
                        console.log("ok");
                        }
                        else {
                            words[i].setAttribute("style", "display:none;");
                        }
                }

            }
        }
        else {
            alert("Empty filters!");
        }
    });

	$("#changeButton").click(function() {
        var currentFeatures = document.getElementById($("#identificator").val()).getAttribute("data-content").split(';<br />');
        var addedFeature = $("#keys option:selected").text();
		var addedValue = $("#thisFieldValues option:selected").text();
    if (addedFeature != "Any" && addedValue != "Any")
    {

		var isFeatureMultiple = $("#fieldInfo").text();
		var coincidenceFound = false;
		for (let i = 0; i < currentFeatures.length; i++) {
			if (currentFeatures[i] != "") {
				var feature = currentFeatures[i].split(':');
				if (feature[0] == addedFeature) {
					var valuesOfFeature = feature[1].split(';');
					for (let j = 0; j < valuesOfFeature.length; j++){
						if (valuesOfFeature[j] != "") {
							if (valuesOfFeature[j] == addedValue) {
								coincidenceFound = true;
								alert('У слова есть этот признак и это значение!');
								break;
							}
						}
					}
					if (coincidenceFound) break;
					if (isFeatureMultiple != "true") {
						coincidenceFound = true;
						alert('Этому признаку может соответствовать только одно значение!');
						break;
					}
					else {
                        currentFeatures[i] += ";";
                        currentFeatures[i] += addedValue;
                        var new_features = "";
                        for (let feature in currentFeatures) {
                            if (feature != "") {
                                new_features += feature;
                                new_features += ";<br />";
                            }
                        }
                        document.getElementById($("#identificator").val()).setAttribute("data-content", currentFeatures);
                        coincidenceFound = true;
                        alert('Добавлено значение');
						break;
					}
				}
			}
		}
		if (!coincidenceFound) {
            alert('Добавлен признак и присвоено значение.');
            document.getElementById($("#identificator").val()).setAttribute("data-content", document.getElementById($("#identificator").val()).getAttribute("data-content") + addedFeature + ":" + addedValue + ";<br />");
		}
    }
	});

  $('#SaveChanges').click(function() {
    $('changedText').text();
    var words = document.getElementsByClassName('word');
    for (let i = 0; i < words.length; i++) {
      if (words[i].dataset.content == "") {
        $('#changedText').append('{' + words[i].id + '}');
      }
      else {
        $('#changedText').append('{' + words[i].id + ' => ' + words[i].dataset.content + '}');
      }

    }
  });

};

function isMatch(pattern, word) {
    if (pattern.includes('*')) {
        pattern = pattern.split('*').join("\.*");
    }
    pattern = "\^" + pattern + "\$";
    pattern = new RegExp(pattern);
    if (word.match(pattern)) {
        return true;
    }
    else {
        return false;
    }

}

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
