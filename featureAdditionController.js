window.onload = function () { 

    $("#keys").change(function () {
        $("#thisFieldValues").text("");
        var selectedOption = $("#keys option:selected").text();
        if (selectedOption != "Any") {
            $('#thisFieldValues').append("<option>Any</option>");
            $.getJSON("../database/fields/" + selectedOption + ".json", function (data) {
                for (let i = 0; i < data.values.length; i++) {
                    $('#thisFieldValues').append("<option>" + data.values[i] + "</option>");
                }

            });
        }
        else {
            $('#thisFieldValues').append("<option>Any</option>");
        }
        
    });

    $('#addFeature').click(function () {
        var selectedFeature = $("#keys option:selected").text();
        var selectedValue = $("#thisFieldValues option:selected").text();
        if ((selectedFeature != "Any") && (selectedValue != "Any")) {
            if ($("#isNegative").prop("checked")) {
                $('#feature').append("!");
            }
            $('#feature').append(selectedFeature + ":" + selectedValue + " ");
        }
    });

}