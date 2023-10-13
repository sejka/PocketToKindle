var authorizeButton = document.getElementById('authorize-pocket-button');
var authorizeCheckMark = document.getElementById('authorize-checkmark');
var submitButton = document.getElementById('submit-button');
submitButton.onclick = register;
authorizeButton.onclick = authroizeOnPocket;

if (getCookie("pocketkey")) {
    submitButton.disabled = false;
    authorizeButton.disabled = true;
    authorizeCheckMark.classList.remove('hidden');
}

function register() {
    submitButton.disabled = true;
    var xhReq = new XMLHttpRequest();
    xhReq.open("POST",
        "/api/register",
        false);
    xhReq.setRequestHeader("Content-Type", "application/json");

    var kindleEmail = document.getElementById('email-input').value.trim();
    var requestCode = getCookie("pocketkey");

    var data = JSON.stringify({
        "kindleEmail": kindleEmail,
        "requestCode": requestCode
    });
    xhReq.send(data);
    var serverResponse = xhReq.responseText;
    alert(serverResponse);
    eraseCookie('pocketkey');
}

async function authroizeOnPocket() {
    authorizeButton.disabled = true;
    url = "api/register/getinfo";
    let response = await fetch(url);
    response = await response.json();
    document.cookie = `pocketkey=${response.requestCode}`;
    window.location.href = response.registrationLink;
}

function eraseCookie(name) {
    document.cookie = name + '=; Max-Age=0'
}

function getCookie(name) {
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return parts.pop().split(";").shift();
};