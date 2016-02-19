//This recursive function will build a valid XPath in a string and return it, based on a given element.
function getPathTo(element) {
    if (element.id !== '')
        return 'id("' + element.id + '")';
    if (element === document.body)
        return element.tagName;

    var ix = 0;
    var siblings = element.parentNode.childNodes;
    for (var i = 0; i < siblings.length; i++) {
        var sibling = siblings[i];
        if (sibling === element)
            return getPathTo(element.parentNode) + '/' + element.tagName + '[' + (ix + 1) + ']';
        if (sibling.nodeType === 1 && sibling.tagName === element.tagName)
            ix++;
    }
}

var entered = "";

//When a key is pressed, communication is made to the application.
document.onkeypress = function notifyType(event) {
    entered += String.fromCharCode(event.keyCode);

    $.ajax({
        type: "POST",
        url: "http://localhost:8055/",
        data: String.fromCharCode(event.keyCode),
        dataType: "text"
    });
};

//This function tries to find a set of elements on the given page which are very likely to contain the best name/label/title of a page.
//The pageObjectIndicators are a set of selectors (from best to worst) which contain text representing the title.
//More indicators should be found and considered!
function getPageObject() {
    var pageObjectIndicators = ["div.page-header h2", "span.breadcrumb-item.active", "#report-name", "div.reportTitle", ".headText", "td.tab-c-sel a"];
    for (var i = 0; i < pageObjectIndicators.length; i++) {
        var po = "";
        if ($(pageObjectIndicators[i]).length > 1) {
            po = $(pageObjectIndicators[i]).eq(0).text().replace(/[^\w!?]/g, '');
        } else {
            po = $(pageObjectIndicators[i]).text().replace(/[^\w!?]/g, '');
        }
        if (po != "") {
            return po;
        }
    }
    return "Main";
}

//This unused function will find the best selector for an element. This is useful when an element has no id, name, or class fields
//because there may be an element directly behind it which does!
function findBestSelector(element) {
    var id = element.getAttribute("id");
    var name = element.getAttribute("name");
    var cname = element.getAttribute("class");

    if (id.valueOf() == "null" && name.valueOf() == "null" && cname.valueOf() == "null") {
        findBestSelector(element.parent);
    } else {
        alert("Got |" + id + name + cname + "|");
    }
}

var prev_path = 'undefined';
var count = 0;

//Primary function to be called when an element is clicked. Uses getPathTo and a bunch of metadata to communicate with the
//application.
function notifyClick(event) {
    if (event === undefined) event = window.event;
    var target = 'target' in event ? event.target : event.srcElement;

    var root = document.compatMode === 'CSS1Compat' ? document.documentElement : document.body;
    var mxy = [event.clientX + root.scrollLeft, event.clientY + root.scrollTop];

    var path = getPathTo(target);

    if (prev_path.valueOf() == path.valueOf()) {
        return;
    }

    prev_path = path.valueOf();

    var payload = {
        Label: "item" + count,
        Name: "" + target.getAttribute("name"),
        Id: "" + target.getAttribute("id"),
        ClassName: "" + target.getAttribute("class"),
        Page: getPageObject(),
        Node: target.nodeName.valueOf(),
        Type: "" + target.getAttribute("type"),
        Path: path.valueOf()
    };

//    findBestSelector(target);

    $.ajax({
        type: "POST",
        url: "http://localhost:8055/",
        data: JSON.stringify(payload),
        contentType: "application/json; charset=utf-8",
        dataType: "json"
    });

    count++;
    entered = "";
}

//Everytime the script is loaded, each a, img, and div element on the page will be given the notifyClick onClick event.
function loadElems() {
    a_elems = document.getElementsByTagName("a");
    for (i = 0, max = a_elems.length; i < max; i++) {
        a_elems[i].onclick = notifyClick;
    }
    img_elems = document.getElementsByTagName("img");
    for (i = 0, max = img_elems.length; i < max; i++) {
        img_elems[i].onclick = notifyClick;
    }
    div_elems = document.getElementsByTagName("div");
    for (i = 0, max = div_elems.length; i < max; i++) {
        div_elems[i].onclick = notifyClick;
    }
    document.onclick = notifyClick;
}

loadElems();