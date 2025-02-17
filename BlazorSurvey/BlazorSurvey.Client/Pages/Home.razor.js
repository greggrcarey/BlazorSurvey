export function CopyToClipboard(url) {
    window.navigator.clipboard.writeText(url).then(function () {
        alert("Copied to Clipboard");
    })
        .catch(function (error) {
            alert(error);
        });
}

export function ConfirmBeforeDelete() {
    return window.confirm("Are you sure you want to delete this survey? This cannot be undone.");
}