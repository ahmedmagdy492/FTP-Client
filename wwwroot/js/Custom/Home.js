
window.addEventListener('load', function () {
    getConnectionList();
});

const  getConnectionList = () => {
    fetch(url + "Home/GetConnectionList")
        .then(res => res.text())
        .then(res => {
            document.querySelector("#list").innerHTML = res;
        });
}