$(document).on('click', '#clauseExtractionButton', function(){
    const spoilered = $(this).attr("clause");
    const spoiler = $(this).text();
    $(this).attr("clause", spoiler);
    $(this).text(spoilered);
});
