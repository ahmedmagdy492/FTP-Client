let currentPath = "";

const navigateRemote = (conID, path) => {
    fetch(url + `Connection/NavigateRemote?connectionId=${conID}&path=${path}`, {
        method: 'POST'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#remoteContainer").innerHTML = res.remoteFiles.result;
            }
            else {
                alert(res.errors[0]);
            }
        });
}

const back = () => {
    document.querySelector('#fullPathInput').value = decodeURIComponent(currentPath);

    fetch(url + `Connection/BackLocal?path=${currentPath}`, {
        method: 'POST'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#localContainer").innerHTML = res.files.result;
                currentPath = res.currentPath;
            }
            else {
                alert(res.errors[0]);
            }
        });
}

const navigateLocal = (conID, path) => {

    currentPath = path;

    fetch(url + `Connection/NavigateLocal?connectionId=${conID}&path=${path}`, {
        method: 'POST'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#localContainer").innerHTML = res.localFiles.result;
            }
            else {
                alert(res.errors[0]);
            }
        });
}

const enterFullScreen = () => {
    // element which needs to enter full-screen mode
    const element = document.querySelector("#connection-section");

    // make the element go to full-screen mode
    element.requestFullscreen()
        .then(function () {
            // element has entered fullscreen mode successfully
            console.log("element has entered fullscreen mode successfully");
        })
        .catch(function (error) {
            // element could not enter fullscreen mode
            console.log("element could not enter fullscreen mode");
        });
}

const viewContent = (path) => {
    fetch(url + `Connection/ViewContent?path=${path}`, {
        method: 'POST'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#popupContainer").innerHTML = res.fileView.result;
                $("#defaultModal").modal("show");
            }
            else {
                alert(res.errors[0]);
            }
        });
}