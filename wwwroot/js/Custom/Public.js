
const submitForm = (event, errSectionID, successSectionID) => {

    const formData = new FormData();
    const elements = event.target;

    for (let element of elements) {
        formData.append(element.name, element.value);
    }

    fetch(elements.action, {
        method: 'post',
        body: formData
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === false) {
                const parent = document.querySelector(`#${errSectionID}`);
                parent.innerHTML = '';
                for (let err of res.errors) {
                    const newEle = document.createElement('div');
                    newEle.style.color = 'red';
                    newEle.innerHTML = err;
                    parent.appendChild(newEle);
                }
            }
            else {
                const parent = document.querySelector(`#${successSectionID}`);
                parent.innerHTML = '';
                const newEle = document.createElement('span');
                newEle.style.color = 'green';
                newEle.innerHTML = res.errors[0];
                parent.appendChild(newEle);

                setTimeout(() => {
                    location.reload();
                }, 2000);
            }
        });

    return false;
}

const loginForm = (event, errSectionID) => {

    const formData = new FormData();
    const elements = event.target;

    for (let element of elements) {
        formData.append(element.name, element.value);
    }

    fetch(elements.action, {
        method: 'post',
        body: formData
    })
        .then(res => res.json())
        .then(res => {
            if (res.success === false) {
                const parent = document.querySelector(`#${errSectionID}`);
                parent.innerHTML = '';
                for (let err of res.errors) {
                    const newEle = document.createElement('div');
                    newEle.style.color = 'red';
                    newEle.innerHTML = err;
                    parent.appendChild(newEle);
                }
            }
            else {
                location.replace(res.redirectUrl);
            }
        });

    return false;
}

const signOut = () => {
    fetch('./Logout', {
        method: 'post',
        body: {}
    })
        .then(res => res.json())
        .then(res => {
            location.replace(res.redirectUrl);
        });
}