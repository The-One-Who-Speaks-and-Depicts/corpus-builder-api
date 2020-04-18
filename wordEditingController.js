window.onload = function () {

    //получаем идентификатор элемента
    var a = document.getElementById('text');

    //вешаем на него событие
    a.onclick = function () {
        console.log("Initialized!");
        //предотвращаем переход по ссылке href
        return false;
    }
}