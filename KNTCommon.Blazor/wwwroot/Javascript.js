/* Set the width of the side navigation to 250px and the left margin of the page content to 250px */
//function openNav() {
//    document.getElementById("mySidenav").style.width = "250px";
//    document.getElementById("main").style.marginLeft = "250px";

//    document.getElementById("centerBoxProgressBar").style.width = "16vw";
//}

///* Set the width of the side navigation to 0 and the left margin of the page content to 0 */
//function closeNav() {
//    document.getElementById("mySidenav").style.width = "0";
//    document.getElementById("main").style.marginLeft = "0";

//    document.getElementById("centerBoxProgressBar").style.width = "27vw";
//}

// Add event listeners to all anchor tags with class "nav-link"
//var navLinks = document.getElementsByClassName("nav-link");
//for (var i = 0; i < navLinks.length; i++) {
//    navLinks[i].addEventListener("click", closeNav);
//}

//var navLinks = document.GetElementsById("nav-link");
//for (var i = 0; i < navLinks.length; i++) {
//    navLinks[i].addEventListener("click", closeNav);
//}

//var navLinks = document.getElementsByClassName("MainLayoutSidenavRowElement");
//for (var i = 0; i < navLinks.length; i++) {
//    navLinks[i].addEventListener("click", closeNav);
//}
//document.addEventListener("DOMContentLoaded", function () {
//    var sidenavRowElements = document.getElementsByClassName("MainLayoutSidenavRowElement");
//    for (var i = 0; i < sidenavRowElements.length; i++) {
//        sidenavRowElements[i].addEventListener("click", function () {
//            // Add your click event handling logic here
//            // For example, you can close the sidenav
//            openNav()
//        });
//    }
//});

//element.addEventListener("click", openNav());


//MAIN

// Reload current page
function reloadPage() {
    location.reload();
}

let isNavOpen = false;

function openNav() {
    const sidenav = document.getElementById("mySidenav");
    if (isNavOpen) {
        sidenav.classList.remove("sidenav-open");
    }
    else {
        sidenav.classList.add("sidenav-open");
    }
    isNavOpen = !isNavOpen;
}

document.addEventListener("click", function (event) {
    const sidenav = document.getElementById("mySidenav");
    const sidenavbtn = document.getElementById("SidenavButton");

    // do nothinh if menu is closed
    if (!isNavOpen) return;

    // ignore on menu or buton menu click
    if (sidenav.contains(event.target) || sidenavbtn.contains(event.target)) {
        return;
    }

    // Sicer zapri meni
    closeNav();
});

function closeNav() {
    const sidenav = document.getElementById("mySidenav");
    sidenav.classList.remove("sidenav-open");
    isNavOpen = false;
    /*    sidenav.style.width = "0";*/
}

window.getButtonValue = function (buttonId) {
    var button = document.getElementById(buttonId);
    if (button) {
        return button.innerText;
    } else {
        return null;
    }
};

function requestFullScreen() {
    const element = document.documentElement;
    if (element.requestFullscreen) {
        element.requestFullscreen();
    } else if (element.mozRequestFullScreen) { /* Firefox */
        element.mozRequestFullScreen();
    } else if (element.webkitRequestFullscreen) { /* Chrome, Safari and Opera */
        element.webkitRequestFullscreen();
    } else if (element.msRequestFullscreen) { /* IE/Edge */
        element.msRequestFullscreen();
    }
}

window.customFunctions = {
    triggerButtonClick: function (buttonId) {
        var button = document.getElementById(buttonId);
        if (button) {
            button.click();
        }
    }
};

window.openAlphanumericKeyboardModal = function (modalId) {
    var modal = new bootstrap.Modal(document.getElementById(modalId));
    modal.show();
}
window.openNumericKeyboardModal = function () {
    var modal = new bootstrap.Modal(document.getElementById('NumericKeyboardModal'));
    modal.show();
}

// download file
window.downloadFile = (filename, mimeType, base64Data) => {
    try {
        const link = document.createElement("a");
        link.href = "data:" + mimeType + ";base64," + base64Data;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } catch (err) {
        console.error("File download failed:", err);
    }
};

// choose file
function triggerFileInputClick() {
    document.getElementById('fileInput').click();
}

//request fullscreen
function SwitchToFullScreen() {
    // Check if the browser supports the Fullscreen API
    if (document.documentElement.requestFullscreen) {
        try {
            // Request fullscreen mode
            if (!document.documentElement.fullScreen) {
                document.documentElement.requestFullscreen();
                document.getElementById("fullscreenButton").textContent = "Esc Fullscreen";
                document.documentElement.fullScreen = true;
                console.log('Switched to fullscreen mode.');
            }
            else {
                document.exitFullscreen();
                document.getElementById("fullscreenButton").textContent = "Fullscreen";
                document.documentElement.fullScreen = false;
                console.log('Exit fullscreen mode.');
            }
        } catch (error) {
            console.error('Failed to switch to fullscreen mode:', error);
        }
    } else {
        console.warn('Fullscreen mode is not supported in this browser.');
    }
}

//LISTENER LOCAL STORAGE REFRESH COOKIE
document.addEventListener('click', function (event) {
    //let fakeCookie = getFromStorage('KNT');
    //if (fakeCookie != null) {
    //    let usersId = fakeCookie.split(';')[0];
    //    writeToStorage('KNT', fakeCookie);
    //}
    //else {

    //}
    /*console.log('Clicked on:', event.target);*/
});

//LOCAL STORAGE

// Method to write a string value to storage
function writeToStorage(key, value) {
    localStorage.setItem(key, value);
}

// Method to get a string value from storage
function getFromStorage(key) {
    return localStorage.getItem(key);
}

// Method to delete a string value from storage 
function removeFromStorage(key) {
    localStorage.removeItem(key);
}

function openWindow(url, target) {
    window.open(url, target);
}

function closeWindow() {
    window.close();
}

function getCaretPosition(textBoxId) {
    const textBox = document.getElementById(textBoxId);
    if (!textBox) return -1;  // Return -1 if the element is not found
    return textBox.selectionStart || 0;  // Return the caret position (cursor index)
}

function setFocusAndCursor(elementId, cursorIndex) {
    const element = document.getElementById(elementId);
    if (element) {
        // Set focus to the element
        requestAnimationFrame(() => {
            element.focus();
        });

        // Ensure focus is applied before setting the cursor position
        requestAnimationFrame(() => {
            if (element.setSelectionRange) {
                // Set the cursor position (caret) at the specified index
                element.setSelectionRange(cursorIndex, cursorIndex);
            } else if (element.createTextRange) {
                // For older browsers, use this method to set the cursor position
                let range = element.createTextRange();
                range.move('character', cursorIndex);
                range.select();
            }
        });
    } else {
        console.error("Element not found: " + elementId);
    }
}
