window.onload = function () {
    let docsAndTexts = $("#doctexts").text();
    let docsAndTextsSplit = docsAndTexts.split('\n');
    for (let i = 0; i < docsAndTextsSplit.length; i++) {
        if (docsAndTextsSplit[i].includes(':')) {
            let document = "<option>" + docsAndTextsSplit[i].split(':')[0] + "</option>";
            $(document).appendTo('#docs');
        }
    }

    $('#docs').change(function () {
        $('#texts').text("Any");
        let chosenDoc = $("#docs option:selected").text();
        if (chosenDoc != "Any") {
            for (let i = 0; i < docsAndTextsSplit.length; i++) {
                if (docsAndTextsSplit[i].includes(chosenDoc)) {
                    let texts = docsAndTextsSplit[i].split(':')[1].split("||");
                    for (let j = 0; j < texts.length; j++) {
                        let text = "<option>" + texts[j] + "</option>";
                        $(text).appendTo('#texts');
                    }
                }
            }
        }        
    });
}