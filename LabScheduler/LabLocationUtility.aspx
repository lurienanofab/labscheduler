<%@ Page Language="C#" %>

<script runat="server">
</script>

<!doctype html>
<html>
<head>
    <title>Lab Location Utility</title>

    <link rel="stylesheet" href="//ssel-apps.eecs.umich.edu/static/lib/bootstrap4/css/bootstrap.min.css" />
    <link rel="stylesheet" href="scripts/lab-location/lab-location.css" />
</head>
<body>
    <div class="container-fluid mt-2">
        <div class="card">
            <div class="card-header">
                Lab Locations
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-4 mb-2">
                        <input type="text" class="form-control labloc-search-text" placeholder="Search..." />
                    </div>
                </div>
                <div class="lab-locations" data-ajax-url="ajax/lablocations.ashx">
                    <div class="row">
                        <div class="col-4">
                            <div class="input-group mb-2">
                                <div class="input-group-prepend">
                                    <select class="custom-select add-labloc-lab-select">
                                        <option value="1">Clean Room</option>
                                        <option value="9">ROBIN</option>
                                    </select>
                                </div>
                                <input type="text" class="form-control add-labloc-text" />
                                <div class="input-group-append">
                                    <button class="btn btn-outline-secondary add-labloc" type="button">Add</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-6">
                            <div class="lab-location-items">
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>
    <script src="//ssel-apps.eecs.umich.edu/static/lib/popper/2.9.0/umd/popper.min.js"></script>
    <script src="//ssel-apps.eecs.umich.edu/static/lib/bootstrap4/js/bootstrap.min.js"></script>
    <script src="scripts/lab-location/lab-location.js"></script>

    <script>
        var lablocs = $(".lab-locations").lablocation({
            "oninit": function (instance) {
                var search = $(".labloc-search-text");
                search.off("keyup").on("keyup", function () {
                    var searchText = search.val();
                    instance.search(searchText);
                });
            }
        });
    </script>
</body>
</html>
