window.onload = function () {
    let docsAndTexts = $("#doctexts").text();
    let docsAndTextsSplit = docsAndTexts.split('\n');
    for (let i = 0; i < docsAndTextsSplit.length; i++) {
        if (docsAndTextsSplit[i].includes(':')) {
            let document = "<option>" + docsAndTextsSplit[i] + "</option>";
            $(document).appendTo('#manuscriptList');
        }
    }
}
