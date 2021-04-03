jsons = [];

$(document).ready(function () {
    $(".word").dblclick(function () {
        $('#tagging').text("");
        $('#info').text("");
        $('#info').append("Lexeme:");
        $('#info').append(this.innerHTML);
        $('#info').append("<br /><br /> Features:<br />");
        var taggings = $(this).attr('data-content').split('***');
        for (let t = 0; t < taggings.length; t++) {
            if (taggings[t] != "") {
                $('#info').append("Tagging " + t + "<br />");
                var features = taggings[t].split('<br />');
                for (let i = 0; i < features.length; i++) {
                    if (features[i] != "") {
                        $('#info').append('<span><span>' + features[i] + '</span><button class=\"deleteTaggedButton\" type=\"button\">Удалить</button></span><br />');
                    }
                }
            }            
        }
        
        $('#info').append("<br /><br /><br />");
        $("#identificator").text("");
        $("#identificator").append($(this).attr('id'));
        $("#keys").text("");
        $("#keys").append("<option>Any</option>");
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].type == "Realization") {
                $("#keys").append("<option>" + jsons[i].name + "</option>");
            }
        }
        $("#similarTagging").css("opacity", 1.0);
        $("#similarTaggingLabel").css("opacity", 1.0);

        $("#editTagging").css("opacity", 1.0);
        $("#editTaggingLabel").css("opacity", 1.0);

        $("#tagNumber").css("opacity", 1.0);
        $("#tagNumberLabel").css("opacity", 1.0);

        $(".deleteTaggedButton").click(function () {

            var words = $(".word");
            for (let i = 0; i < words.length; i++) {
                if (words[i].id == $("#identificator").val()) {
                    var features = $(words[i]).attr('data-content').split('<br />');
                    for (let j = 0; j < features.length; j++) {
                        if ((features[j] != "") && (features[j] == $(this).parent().children()[0].innerText)) {
                            words[i].setAttribute('data-content', words[i].getAttribute('data-content').replace(features[j] + "<br />", ""));
                        }
                    }
                }
            }
            $(this).parent().remove();
        })
    });

    $('.grapheme').bind('contextmenu', show_graphemes);

    function show_graphemes(event) {
        event.preventDefault();
        $('#tagging').text("");
        $('#info').text("");
        $('#info').append("Grapheme:");
        $('#info').append(this.innerHTML);
        $('#info').append("<br /><br /> Features:<br />");
        var features = $(this).attr('data-content').split('<br />');
        for (let i = 0; i < features.length; i++) {
            if (features[i] != "") {
                $('#info').append('<span><span>' + features[i] + '</span><button class=\"deleteTaggedButton\" type=\"button\">Удалить</button></span><br />');
            }
        }
        $('#info').append("<br /><br /><br />");
        $("#identificator").text("");
        $("#identificator").append($(this).attr('id'));
        $("#keys").text("");
        $("#keys").append("<option>Any</option>");
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].type == "Grapheme") {
                $("#keys").append("<option>" + jsons[i].name + "</option>");
            }
        }
        if ($("#similarTagging").css("opacity") == 1) {
            $("#similarTagging").css("opacity", 0.0);
            $("#similarTaggingLabel").css("opacity", 0.0);
            $("#editTagging").css("opacity", 0.0);
            $("#editTaggingLabel").css("opacity", 0.0);

            $("#tagNumber").css("opacity", 0.0);
            $("#tagNumberLabel").css("opacity", 0.0);
        }
        $(".deleteTaggedButton").click(function () {
            var graphemes = $(".grapheme");
            for (let i = 0; i < graphemes.length; i++) {
                if (graphemes[i].id == $("#identificator").val()) {
                    var features = $(graphemes[i]).attr('data-content').split('<br />');
                    for (let j = 0; j < features.length; j++) {
                        if ((features[j] != "") && (features[j] == $(this).parent().children()[0].innerText)) {
                            graphemes[i].setAttribute('data-content', graphemes[i].getAttribute('data-content').replace(features[j] + "<br />", ""));
                        }
                    }
                }
            }
            $(this).parent().remove();
        })
    }


    var values = document.getElementById('values');
    splitValues = values.innerText.split('|');
    for (var i = 0; i < splitValues.length; i++) {
        $.getJSON("/database/fields/" + splitValues[i], function (data) {
            if (data.type != "Document" && data.type != "Text") {
                jsons.push(data);
                if (data.type == "Realization") {
                    $("#keysFilter").append("<option>" + data.name + "</option>");
                }
            }
        });
    }
    jsons.sort((a, b) => (a.name > b.name) ? 1 : ((b.name > a.name) ? -1 : 0));


    $("#keys").change(function () {
        $("#thisFieldValues").text("");
        $("#userValue").text("");
        $("#connected").text("");
        var selectedOption = $("#keys option:selected").text();
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
                                            $('#connected').append("<button id=\"additionalFeatureButton\">Добавить связанный признак в разметку</button> <br /> <br />")
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
                                    $("#additionalFeatureButton").click(function () {

                                        var category = $("#connectedFields option:selected").text();
                                        if (category != "Any") {
                                            if ($("#connectedUserValue").css("opacity") == 1) {
                                                if ($("#connectedUserValue").text() != "") {
                                                    $("#tagging").append("<span class=\"tag\">" + category + ":" + $("#connectedUserValue").text() + " <button class=\"deleteTaggingButton\" type=\"button\">Удалить</button></span><br />");
                                                }

                                            }
                                            else if ($("#connectedRestrictedValue").css("opacity") == 1) {
                                                if ($("#connectedRestrictedValue option:selected").text() != "Any") {
                                                    $("#tagging").append("<span class=\"tag\">" + category + ":" + $("#connectedRestrictedValue option:selected").text() + " <button class=\"deleteTaggingButton\" type=\"button\">Удалить</button></span><br />");
                                                }
                                            }

                                            $(".deleteTaggingButton").click(function () {
                                                $(this).parent().remove();
                                            });
                                        }
                                    });
                                });
                            }
                        }
                    }
                }
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
        if ($("#isNegative").is(':checked')) {
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
                if (selectedWord.length > 0) {
                    if (!isMatch(selectedWord, words[i].innerText)) {
                        words[i].setAttribute("style", "opacity:0.25;");
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
                        words[i].setAttribute("style", "opacity:0.25;");
                    }
                }

            }
        }
        else {
            alert("Empty filters!");
        }
    });
    $("#buttonFilterDeletion").click(function () {
        var words = document.getElementsByClassName('word');
        for (let i = 0; i < words.length; i++) {
            words[i].setAttribute("style", "opacity:1.0;");
        }
    });

    function changing(id, isLexeme, newTagging) {
        var newFeatures = document.getElementsByClassName("tag");
        for (let i = 0; i < newFeatures.length; i++) {
            var currentFeatures = document.getElementById(id).getAttribute("data-content").split(';<br />');
            var feature = newFeatures[i].textContent.match(/.*\s/g)[0];
            var addedFeature = feature.split(':')[0];
            var addedValue = feature.split(':')[1];
            var isFeatureMultiple = false;
            for (let i = 0; i < jsons.length; i++) {
                if (jsons[i].name == addedFeature) {
                    isFeatureMultiple = jsons[i].isMultiple;
                    break;
                }
            }
            var coincidenceFound = false;
            for (let i = 0; i < currentFeatures.length; i++) {
                if (currentFeatures[i] != "") {
                    var feature = currentFeatures[i].split(':');
                    if (feature[0] == addedFeature) {
                        var valuesOfFeature = feature[1].split(';');
                        for (let j = 0; j < valuesOfFeature.length; j++) {
                            if (valuesOfFeature[j] != "") {
                                var multipleFeatures = valuesOfFeature[j].split(' ,');
                                for (let m = 0; m < multipleFeatures.length; m++) {
                                    if (multipleFeatures[m].trim() == addedValue.trim()) {
                                        coincidenceFound = true;
                                        alert('У клаузы есть один из признаков и одно из значений!');
                                        break;
                                    }
                                }
                                if (coincidenceFound) break;
                                
                            }
                        }
                        if (coincidenceFound) break;
                        if (isFeatureMultiple == false) {
                            coincidenceFound = true;
                            alert('Одному из признаков может соответствовать только одно значение!');
                            break;
                        }
                        else {
                            currentFeatures[i] += "," + addedValue;
                            let new_features = "";
                            for (let j = 0; j < currentFeatures.length; j++) {
                                if (currentFeatures[j] != "") {
                                    if (currentFeatures[j].endsWith(";<br />")) {
                                        new_features += currentFeatures[j];
                                    }
                                    else {
                                        new_features += currentFeatures[j] + ";<br />";
                                    }
                                }

                            }
                            if (isLexeme == true) {
                                newFeatures += "***";
                            }
                            document.getElementById(id).setAttribute("data-content", new_features);
                            coincidenceFound = true;
                            break;
                        }
                    }
                }
            }
            if (!coincidenceFound) {
                if (isLexeme == true) {
                    document.getElementById(id).setAttribute("data-content", document.getElementById(id).getAttribute("data-content") + addedFeature + ":" + addedValue + ";<br />" + "***");
                }
                else {
                    document.getElementById(id).setAttribute("data-content", document.getElementById(id).getAttribute("data-content") + addedFeature + ":" + addedValue + ";<br />")
                }
            }
        }
    }

    function check_ids(id_origin, id_transfer) {
        if (id_origin == id_transfer) {
            return false;
        }
        if (id_origin.split('|')[2] < id_transfer.split('|')[2]) {
            return true;
        }
        else if (id_origin.split('|')[2] > id_transfer.split('|')[2]) {
            return false;
        }
        else {
            if (id_origin.split('|')[3] < id_transfer.split('|')[3]) {
                return false;
            }
            else {
                return true;
            }
        }
    }

    $("#changeButton").click(function () {
        id = $("#identificator").val()
        if (!$('#info').text().startsWith("Lexeme")) {
            changing(id, false, false);
        }
        else {
            changing(id, true, true);
            if ($('#similarTagging').prop('checked')) {
                words = $(".word");
                var wordText = "";
                for (let i = 0; i < words.length; i++) {
                    if (words[i].id == id) {
                        wordText = words[i].innerText;
                    }
                }
                for (let i = 0; i < words.length; i++) {
                    if (words[i].innerText.trim() == wordText.trim() && check_ids(id, words[i].id)) {
                        changing(words[i].id);
                    }
                }
            }
        }
        $('#info').text("");
        $('#identificator').text("");
        $('#tagging').text("");
        $('#keys option:first').prop('selected', true);
        if ($("#userValue").css("opacity") == 1) {
            $("#userValue").css("opacity", 0.0);
            $("#userValue").val("");
        }
        else if ($("#thisFieldValues").css("opacity") == 1) {
            $("#thisFieldValues").css("opacity", 0.0);
            $('#thisFieldValues option:first').prop('selected', true);
        }
        $("#connected").text("");
        alert('Внесение завершено!');

    });

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

    $('#SaveChanges').bind('click', save_changes);

    function save_changes(event) {
        event.preventDefault();
        $('changedText').text();
        var clauses = document.getElementsByClassName('clause');
        for (let i = 0; i < clauses.length; i++) {
            if (clauses[i].dataset.content == "") {
                $('#changedText').append('{' + clauses[i].id + '}');
            }
            else {
                $('#changedText').append('{' + clauses[i].id + ' => ' + clauses[i].dataset.content + '}');
            }

        }
        var words = document.getElementsByClassName('word');
        for (let i = 0; i < words.length; i++) {
            if (words[i].dataset.content == "") {
                $('#changedText').append('{' + words[i].id + '}');
            }
            else {
                $('#changedText').append('{' + words[i].id + ' => ' + words[i].dataset.content + '}');
            }

        }
        var graphemes = document.getElementsByClassName('grapheme')
        for (let i = 0; i < graphemes.length; i++) {
            if (graphemes[i].dataset.content == "") {
                $('#changedText').append('{' + graphemes[i].id + '}');
            }
            else {
                $('#changedText').append('{' + graphemes[i].id + ' => ' + graphemes[i].dataset.content + '}');
            }

        }
        alert('Изменения сохранены');
    }

    $(".clauseButton").click(function () {
        $('#tagging').text("");
        var tokens = $(this).parent().children();
        $('#info').text("");
        $('#info').append("Clause:");
        for (let i = 0; i < tokens.length; i++) {
            if ($(tokens[i]).hasClass('word')) {
                $('#info').append($(tokens[i]).text());
            }
        }
        $('#info').append("<br /><br /> Features:<br />");
        var features = $(this).parent().attr('data-content').split('<br />');
        for (let i = 0; i < features.length; i++) {
            if (features[i] != "") {
                $('#info').append('<span><span>' + features[i] + '</span><button class=\"deleteTaggedButton\" type=\"button\">Удалить</button></span><br />');
            }
        }
        $('#info').append("<br /><br /><br />");
        $("#identificator").text("");
        $("#identificator").append($(this).parent().attr('id'));
        $("#keys").text("");
        $("#keys").append("<option>Any</option>");
        for (var i = 0; i < jsons.length; i++) {
            if (jsons[i].type == "Clause") {
                $("#keys").append("<option>" + jsons[i].name + "</option>");
            }
        }
        if ($("#similarTagging").css("opacity") == 1) {
            $("#similarTagging").css("opacity", 0.0);
            $("#similarTaggingLabel").css("opacity", 0.0);

            $("#editTagging").css("opacity", 0.0);
            $("#editTaggingLabel").css("opacity", 0.0);

            $("#tagNumber").css("opacity", 0.0);
            $("#tagNumberLabel").css("opacity", 0.0);
        }

        $(".deleteTaggedButton").click(function () {

            var clauses = $(".clause");
            for (let i = 0; i < clauses.length; i++) {
                if (clauses[i].id == $("#identificator").val()) {
                    var features = $(clauses[i]).attr('data-content').split('<br />');
                    for (let j = 0; j < features.length; j++) {
                        if ((features[j] != "") && (features[j] == $(this).parent().children()[0].innerText)) {
                            clauses[i].setAttribute('data-content', clauses[i].getAttribute('data-content').replace(features[j] + "<br />", ""));
                        }
                    }
                }
            }
            $(this).parent().remove();
        })
    });

    $("#mainFeatureButton").click(function () {

        var category = $("#keys option:selected").text();
        if (category != "Any") {
            if ($("#userValue").css("opacity") == 1) {
                if ($("#userValue").val() != "") {
                    $("#tagging").append("<span class=\"tag\">" + category + ":" + $("#userValue").val() + " <button class=\"deleteTaggingButton\" type=\"button\">Удалить</button></span><br />");
                }

            }
            else if ($("#thisFieldValues").css("opacity") == 1) {
                if ($("#thisFieldValues option:selected").text() != "Any") {
                    $("#tagging").append("<span class=\"tag\">" + category + ":" + $("#thisFieldValues option:selected").text() + " <button class=\"deleteTaggingButton\" type=\"button\">Удалить</button></span><br />");
                }
            }
        }

        $(".deleteTaggingButton").click(function () {
            $(this).parent().remove();
        });
    });




});