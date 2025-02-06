export function CopyToClipboard(url) {
    window.navigator.clipboard.writeText(url).then(function () {
        alert("Copied to Clipboard");
    })
        .catch(function (error) {
            alert(error);
        });
}
