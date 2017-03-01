/*
  Copyright 2017 University of Michigan

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

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