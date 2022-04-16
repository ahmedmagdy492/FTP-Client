let currentPath = "";
let remoteCurrentPath = "";

// drag and drop
function handleDragStart(e) {
    this.style.opacity = '0.4';
    e.dataTransfer.effectAllowed = 'copy';
    e.dataTransfer.setData('application/json', JSON.stringify({
        connectionID: this.dataset.connectionId,
        filePath: this.dataset.filePath
    }));
}

function handleDragEnd(e) {
    this.style.opacity = '1';
}

function handleDragOver(e) {
    e.preventDefault();
    return false;
}

function handleDragEnter(e) {
}

function handleDragLeave(e) {
}

function handleDrop(e) {
    e.stopPropagation(); // stops the browser from redirecting.

    let data = JSON.parse(e.dataTransfer.getData('application/json'));

    $.blockUI({
        message: `<div class="d-flex justify-content-center align-items-center"><p class="mr-50 mb-0">Uploading...</p> <div class="spinner-grow spinner-grow-sm text-white" role="status"></div> </div>`,
        css: {
            backgroundColor: 'transparent',
            color: '#fff',
            border: '0'
        },
        overlayCSS: {
            opacity: 0.5
        },
        onBlock: function () {
            fetch(url + `Connection/UploadFile?connectionId=${data.connectionID}&remoteServerPath=${remoteCurrentPath}&localFilePath=${data.filePath}`, {
                method: 'post'
            })
                .then(res => res.json())
                .then(res => {
                    // hide loader
                    $.unblockUI();
                    if (res.success === true) {
                        document.querySelector('#remoteContainer').innerHTML = res.remotefiles.result;
                        setupDragAndDropEvents();
                    }
                    else {
                        alert(res.errors[0]);
                    }
                });
        },
    });

    return false;
}

function setupDragAndDropEvents() {
    const allDraggableItems = [...document.querySelectorAll('.draggable-content')];
    const container = document.querySelector('#remote-drop');
    container.addEventListener('dragstart', handleDragStart);
    container.addEventListener('dragend', handleDragEnd);
    container.addEventListener('dragenter', handleDragEnter);
    container.addEventListener('dragleave', handleDragLeave);
    container.addEventListener('dragover', handleDragOver);
    container.addEventListener('drop', handleDrop);


    allDraggableItems.forEach(i => {
        i.addEventListener('dragstart', handleDragStart);
        i.addEventListener('dragend', handleDragEnd);
        i.addEventListener('dragenter', handleDragEnter);
        i.addEventListener('dragleave', handleDragLeave);
        i.addEventListener('dragover', handleDragOver);
        i.addEventListener('drop', handleDrop);
    });
}

const navigateRemote = (conID, path) => {

    remoteCurrentPath = path;

    document.querySelector("#remoteContainer").innerHTML = "<div class='d-flex justify-content-center'><img src='/imgs/​​Iphone-spinner-1.gif' class='mt-4' width='35' height='35' /></div>";

    fetch(url + `Connection/NavigateRemote?connectionId=${conID}&path=${path}`, {
        method: 'POST'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#remoteContainer").innerHTML = res.remoteFiles.result;
                document.querySelector("#connection-status").style.color = "green";
                document.querySelector("#connection-status").innerHTML = "Connected";

                if (path !== '')
                    document.querySelector("#fullPathInputRemote").value = decodeURIComponent(path);

                setupDragAndDropEvents();
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

const backRemote = (conId) => {

    if (remoteCurrentPath === "")
        return;

    document.querySelector("#remoteContainer").innerHTML = "<div class='d-flex justify-content-center'><img src='/imgs/​​Iphone-spinner-1.gif' class='mt-4' width='35' height='35' /></div>";

    fetch(url + `Connection/BackRemote?conId=${conId}&path=${remoteCurrentPath}`, {
        method: 'POST'
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === true) {
                document.querySelector("#remoteContainer").innerHTML = res.files.result;
                remoteCurrentPath = res.currentPath;

                setupDragAndDropEvents();
            }
            else {
                alert(res.errors[0]);
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

                setupDragAndDropEvents();
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

                setupDragAndDropEvents();
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
    const selectAllCheckBox = event.target;
    const allCheckBoxes = document.querySelectorAll(".local-check-box");

    if (selectAllCheckBox.checked === true) {
        allCheckBoxes.forEach(c => {
            c.checked = true;
        });
    }
    else {
        allCheckBoxes.forEach(c => {
            c.checked = false;
        });
    }
}

const checkOrUnCheckOne = (event) => {
    const selectAllCheckBox = document.querySelector('#checkbox-select-all');
    const allCheckBoxes = document.querySelectorAll(".local-check-box");
    const checkedCheckBoxes = [...allCheckBoxes].filter(i => i.checked);

    if (selectAllCheckBox.checked) {
        selectAllCheckBox.checked = false;
    }
    else if (allCheckBoxes.length === checkedCheckBoxes.length) {
        selectAllCheckBox.checked = true;
    }
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

// remote server select all functions
const remoteCheckOrUnCheckAll = (event) => {
    const selectAllCheckBox = event.target;
    const allCheckBoxes = document.querySelectorAll(".remote-check-box");

    if (selectAllCheckBox.checked === true) {
        allCheckBoxes.forEach(c => {
            c.checked = true;
        });
    }
    else {
        allCheckBoxes.forEach(c => {
            c.checked = false;
        });
    }
}

const remoteCheckOrUnCheckOne = (event) => {
    const selectAllCheckBox = document.querySelector('#remote-check-all');
    const allCheckBoxes = document.querySelectorAll(".remote-check-box");
    const checkedCheckBoxes = [...allCheckBoxes].filter(i => i.checked);

    if (selectAllCheckBox.checked) {
        selectAllCheckBox.checked = false;
    }
    else if (allCheckBoxes.length === checkedCheckBoxes.length) {
        selectAllCheckBox.checked = true;
    }
}

// uploading and downloading
const uploadfiles = (connectionID) => {
    
    // show loader
    let allSelectedFiles = [...document.querySelectorAll('.local-check-box')].filter(check => check.checked);

    if (allSelectedFiles.length === 0) {
        alert('please select at most 3 files to upload');
        return;
    }

    if (allSelectedFiles.length > 3) {
        alert('cannot send more than 3 files at once');
        return;
    }

    $.blockUI({
        message: `<div class="d-flex justify-content-center align-items-center"><p class="mr-50 mb-0">Uploading...</p> <div class="spinner-grow spinner-grow-sm text-white" role="status"></div> </div>`,
        css: {
            backgroundColor: 'transparent',
            color: '#fff',
            border: '0'
        },
        overlayCSS: {
            opacity: 0.5
        },
        onBlock: function () {
            allSelectedFiles.forEach(file => {
                fetch(url + `Connection/UploadFile?connectionId=${connectionID}&remoteServerPath=${remoteCurrentPath}&localFilePath=${file.dataset.filePath}`, {
                    method: 'post'
                })
                    .then(res => res.json())
                    .then(res => {
                        // hide loader
                        $.unblockUI();
                        if (res.success === true) {
                            document.querySelector('#remoteContainer').innerHTML = res.remotefiles.result;
                        }
                        else {
                            alert(res.errors[0]);
                        }
                    });
            });
        },
    });
}

const downloadfiles = (connectionID) => {

    // show loader
    let allSelectedFiles = [...document.querySelectorAll('.remote-check-box')].filter(check => check.checked);

    if (allSelectedFiles.length === 0) {
        alert('please select at most 3 files to download');
        return;
    }

    if (allSelectedFiles.length > 3) {
        alert('cannot download more than 3 files at once');
        return;
    }

    $.blockUI({
        message: `<div class="d-flex justify-content-center align-items-center"><p class="mr-50 mb-0">Downloading...</p> <div class="spinner-grow spinner-grow-sm text-white" role="status"></div> </div>`,
        css: {
            backgroundColor: 'transparent',
            color: '#fff',
            border: '0'
        },
        overlayCSS: {
            opacity: 0.5
        },
        onBlock: function () {
            allSelectedFiles.forEach(file => {
                fetch(url + `Connection/DownloadFile?connectionId=${connectionID}&remoteServerPath=${file.dataset.filePath}&localFilePath=${currentPath}`, {
                    method: 'post'
                })
                    .then(res => res.json())
                    .then(res => {
                        // hide loader
                        $.unblockUI();
                        if (res.success === true) {
                            document.querySelector('#localContainer').innerHTML = res.localFiles.result;
                        }
                        else {
                            alert(res.errors[0]);
                        }
                    });
            });
        },
    });
}