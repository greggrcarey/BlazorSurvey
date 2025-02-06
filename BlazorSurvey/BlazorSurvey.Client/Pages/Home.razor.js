export function CopyToClipboard(url) {
    window.navigator.clipboard.writeText(url).then(function () {
        alert("Copied to Clipboard");
    })
        .catch(function (error) {
            alert(error);
        });
}

/*
window.clipboardCopy = {
    copyText: function (text) {
        navigator.clipboard.writeText(text).then(function () {
            alert("Copied to clipboard!");
        })
            .catch(function (error) {
                alert(error);
            });
    }
};*/