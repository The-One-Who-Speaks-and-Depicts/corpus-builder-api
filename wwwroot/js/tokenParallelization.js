$(document).on('click', '.token', function(){
    const id = $(this).attr("id").split('|')[0] + "|" + $(this).children()[0].id.split('|')[1] + "|" + $(this).children()[0].id.split('|')[2] + "|" + $(this).children()[0].id.split('|')[3];
    const word = $(this).children()[0].innerText;
    const representation = "{" + id + " - " + word + "};";
    $("#intermediate").append(representation);
});

$(document).on('click', '#transferText', function(){
    const parallelizedToken = $("#intermediate").text();
    $("#intermediate").text("");
    $("#final").append("<div>" + parallelizedToken + "<button id=\"deleteTemporary\">[[Delete]]</button></div>");
});

$(document).on('click', "#deleteTemporary", function() {
    $(this).parent().remove();
})

$(document).on('click', "#transferClick", function(e) {
    const parallelized = $("#final").text();
    $("#finalInput").val(parallelized);
})
