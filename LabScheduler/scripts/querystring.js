QueryString = new function () {
    var urlParams = null;

    this.get = function (key) {
        var qs = this.getJSON();

        for (var k in qs) {
            if (key.toLowerCase() === k.toLocaleLowerCase())
                return qs[k];
        }

        return null;
    };

    //adapted from:
    //http://stackoverflow.com/questions/901115/how-can-i-get-query-string-values-in-javascript#2880929
    this.getJSON = function () {
        // only need to do this once
        if (!urlParams) {
            var match,
                pl = /\+/g,  // Regex for replacing addition symbol with a space
                search = /([^&=]+)=?([^&]*)/g,
                decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); },
                query = window.location.search.substring(1);

            urlParams = {};
            while (match = search.exec(query))
                urlParams[decode(match[1])] = decode(match[2]);
        }

        return urlParams;
    }
};