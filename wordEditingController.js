window.onload = function () {

    //получаем идентификатор элемента
    var a = document.getElementById('word');

    //вешаем на него событие
    a.onclick = function () {
        //производим какие-то действия
        if (this.innerHTML == 'On') this.innerHTML = 'Off';
        else this.innerHTML = 'On';
        //предотвращаем переход по ссылке href
        return false;
    }
}