let currentPath = "";

const navigateRemote = (conID, path) => {
    fetch(url + `Connection/NavigateRemote?connectionId=${conID}&path=${path}`, {
        method: 'POST'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#remoteContainer").innerHTML = res.remoteFiles.result;
                document.querySelector("#connection-status").style.color = "green";
                document.querySelector("#connection-status").innerHTML = "Connected";
            }
            else {
                document.querySelector("#remoteContainer").style.color = "red";
                document.querySelector("#remoteContainer").innerHTML = res.errors[0] + " Please Reconnect";
                document.querySelector("#connection-status").style.color = "red";
                document.querySelector("#connection-status").innerHTML = "Not Connected";
                //alert(res.errors[0]);
            }
        });
}

const back = () => {

    if (currentPath === "")
        return;

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
                if (path !== '')
                    document.querySelector("#fullPathInput").value = decodeURIComponent(path);
            }
            else {
                //document.querySelector("#localContainer").innerHTML = res.errors[0];
                alert(res.errors[0]);
            }
        });
}

const enterFullScreen = () => {
    // element which needs to enter full-screen mode
    const element = document.querySelector("#connection-section");

    // make the element go to full-screen mode
    element.requestFullscreen({ navigationUI: "show" })
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
                if (document.fullscreenElement !== null) {
                    document.exitFullscreen()
                        .then(function () {
                            // element has exited fullscreen mode
                            document.querySelector("#popupContainer").innerHTML = res.fileView.result;
                            $("#defaultModal").modal("show");
                        })
                        .catch(function (error) {
                            // element could not exit fullscreen mode
                            // error message
                            console.log(error.message);
                        });
                }
                else {
                    document.querySelector("#popupContainer").innerHTML = res.fileView.result;
                    $("#defaultModal").modal("show");
                }
            }
            else {
                alert(res.errors[0]);
            }
        });
}

const checkOrUnCheckAll = (event) => {

}

const checkOrUnCheckOne = (event) => {

}

const saveFile = (path) => {
    const formData = new FormData();
    formData.append("path", path);
    formData.append("content", document.querySelector("#content").value);

    fetch(url + "Connection/SaveFile", {
        method: "post",
        body: formData,
        contentType: 'multipart/form-data'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#save-file-msg").style.color = "green";
                document.querySelector("#save-file-msg").innerHTML = "File has been Saved";
                scrollBy(0, -1000);

                setTimeout(() => {
                    $("#defaultModal").modal('hide');
                }, 2000);
            }
            else {
                document.querySelector("#save-file-msg").style.color = "red";
                document.querySelector("#save-file-msg").innerHTML = res.errors[0];
            }
        });
}