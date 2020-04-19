window.onload = function () {
    $(".word").dblclick(function () {
        $('#info').text("");
        $('#info').append(this.innerHTML);
    });
    
}

