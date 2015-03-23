function getQuerystring(key, querystring) {
    var defaultvalue = "";
    key = key.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + key + "=([^&#]*)");
    var qs = "";
    if (querystring == null) {
        qs = regex.exec(window.location.href);
    }
    else {
        qs = regex.exec(querystring);
    }
    if (qs == "" || qs == null)
        return defaultvalue;
    else
        return qs[1];
}

var id;
var savedurl;

function ClickTracking(query) {
    var pos = 1;
    var all = document.getElementsByTagName("*");
    for (var i = 0; i < all.length; ++i) {
        var itemId = all[i].getAttribute('data-ts');
        if (itemId != null) {
            itemId = (itemId == "" ? "0" : itemId);
            var aItem = all[i];
            var querystring = id + "/?hit=" + itemId + "&hit.pos=" + pos + "&hit.url=" + aItem.getAttribute("href") + "&hit.name=" + aItem.innerHTML + "&q=" + query;
            aItem.setAttribute("data-ts", querystring);
            aItem.onclick = function () {
                var querystring = this.getAttribute("data-ts");
                //                var object = {id:id, hit: getQuerystring("hit", querystring), pos: getQuerystring("hit.pos", querystring), url: getQuerystring("hit.url", querystring), name: getQuerystring("hit.name", querystring), q: getQuerystring("q", querystring) };
                var url = savedurl;
                try {
                    if ($.browser.msie && window.XDomainRequest) {
                        // Use Microsoft XDR
                        var req = new XDomainRequest();
                    }
                    else if (window.XMLHttpRequest) {
                        //var querystring = this.dataset.ts;
                        var req = new XMLHttpRequest();
                    }
                    else {

                        //var querystring = this.dataset.ts;
                        var req = new ActiveXObject("Microsoft.XMLHTTP");
                    }
                    req.open("GET", encodeURI(url + querystring), false);
                    req.send();
                } catch (err) {
                    console.log(err);
                }
            }
            ++pos;
        }

    }
}

function StartTrufflerStats(url, statsObject) {
    savedurl = url;
    var prevSearch = getCookie("trufflerstats");
    if (prevSearch == null || prevSearch != query) {
        if (statsObject.q != null) {
            //Need new Id for this.
            id = Math.floor(Math.random() * 100000000);
            SendStatsObject(url, statsObject);
            setCookie("trufflerstats", query);
            setCookie("trufflerid", id);
            ClickTracking(statsObject.q);
        }
        //ELSE Can't send anything..
    }
    else if (statsObject.q != null) {
        id = getCookie("trufflerid");
        ClickTracking(statsObject.q);
    }
}

function EnableTrufflerStats(key,statsObject,url) {
    EnableTrufflerStats(url, statsObject);
}

function StartTrufflerStats(url, statsObject) {
    window.onload = function () {
        savedurl = url;
        var prevSearch = getCookie("trufflerstats");
        if (prevSearch == null || prevSearch != query) {
            if (statsObject.q != null) {
                //Need new Id for this.
                id = Math.floor(Math.random() * 100000000);
                SendStatsObject(savedurl, statsObject);
                setCookie("trufflerstats", query);
                setCookie("trufflerid", id);
                ClickTracking(statsObject.q);
            }
            //ELSE Can't send anything..
        }
        else if (statsObject.q != null) {
            id = getCookie("trufflerid");
            ClickTracking(statsObject.q);
        }
    }
}

function SendStatsObject(url, statsObject) {
    if ($.browser.msie && window.XDomainRequest) {
        // Use Microsoft XDR
        var req = new XDomainRequest();

    }
    else if (window.XMLHttpRequest) {
        var req = new XMLHttpRequest();
    }
    else {

        var req = new ActiveXObject("Microsoft.XMLHTTP");
    }
    var trackUrl = url +
        (statsObject.id == null ? id + "/" : statsObject.id + "/") +
        (statsObject.q == null ? "" : "?q=" + statsObject.q) +
            (statsObject.hits == null ? "" : "&q.hits=" + statsObject.hits) +
                (statsObject.took == null ? "" : "&q.took=" + statsObject.took) +
                    (statsObject.showhits == null ? "" : "&q.showhits=" + statsObject.showhits) +
                        (statsObject.language == null ? "" : "&q.language=" + statsObject.language) +
                            (statsObject.page == null ? "" : "&page=" + statsObject.page) +
                                (statsObject.hit == null ? "" : "&hit=" + statsObject.hit) +
                                    (statsObject.name == null ? "" : "&hit.name=" + statsObject.name) +
                                        (statsObject.pos == null ? "" : "&hit.pos=" + statsObject.pos) +
                                            (statsObject.url == null ? "" : "&hit.url=" + statsObject.url) +
                                                (statsObject.timestamp == null ? "" : "&hit.timestamp=" + statsObject.timestamp) +
                                                    (statsObject.tags == null ? "" : "&tags=" + statsObject.tags) +
                                                        (statsObject.ip == null ? "" : "&ip=" + statsObject.ip);
    req.open("GET", encodeURI(trackUrl), true);
    req.send();
}


function setCookie(name, value) {
    var d = new Date();
    d.setMinutes(d.getMinutes() + 5);
    var value = escape(value) + "; expires=" + d.toGMTString();
    document.cookie = name + "=" + value + "; path=/";
}


function getCookie(name) {
    var i, x, y, ARRcookies = document.cookie.split(";");
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == name) {
            return unescape(y);
        }
    }
}