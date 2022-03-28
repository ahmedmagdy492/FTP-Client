const getNewConnectionPopup = () => {
    fetch("./NewConnectionPopup", {
        method: 'GET'
    })
        .then(response => {
            return response.text();
        })
        .then(data => {
            document.querySelector('#popupContainer').innerHTML = data;
            $("#defaultModal").modal("show");
        });
}