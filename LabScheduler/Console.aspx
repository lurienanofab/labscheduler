<%--
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
--%>

<%@ Page Language="C#" %>

<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.Net.Mail" %>
<%@ Import Namespace="LNF.CommonTools" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LabScheduler.AppCode" %>

<script runat="server">
    //note: this page does not have a separate CodeBehind file so that server side code can be edited in production

    void Page_Load(object sender, EventArgs e)
    {

    }

    string GetStaticUrl(string path)
    {
        return Utility.GetStaticUrl(path);
    }
</script>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Console</title>

    <!-- Bootstrap -->
    <link href="<%=GetStaticUrl("styles/bootstrap/themes/courier/bootstrap.min.css")%>" rel="stylesheet">

    <style>
        .console {
            position: relative;
            height: 300px;
            background-color: #000;
            color: #4cff00;
        }

        .console-input-container {
            position: absolute;
            bottom: 0;
            width: 100%;
            font-weight: bold;
            background-color: blue;
        }

            .console-input-container .input-group-addon {
                padding: 0 3px 0 3px;
                color: #4cff00;
                font-weight: bold;
                background-color: #000;
                border: none;
            }

            .console-input-container .console-input {
                color: #4cff00;
                border: none;
                background-color: #000;
                font-weight: bold;
                padding: 6px;
                width: 100%;
            }

                .console-input-container .console-input:focus {
                    outline-width: 0;
                    border: none;
                }
    </style>
</head>
<body>
    <div class="container">
        <div class="page-header">
            <h1>Scheduler Console</h1>
        </div>

        <div class="console">


            <div class="input-group console-input-container">
                <span class="input-group-addon">&gt;</span>
                <input type="text" class="zform-control console-input">
            </div>
        </div>
    </div>

    <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="<%=GetStaticUrl("lib/jquery/jquery.min.js")%>"></script>

    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="<%=GetStaticUrl("lib/bootstrap/js/bootstrap.min.js")%>"></script>

    <script>
        (function ($) {
            $.fn.console = function (options) {
                return this.each(function () {
                    var $this = $(this);

                    var opt = $.extend({}, {}, options, $this.data());

                    $this.on("keydown", ".console-input", function (e) {
                        var input = $(this);

                        if (e.keyCode == 13) {
                            e.preventDefault();
                            e.stopPropagation();
                            alert(input.val());
                        }
                    })

                });
            };
        }(jQuery));

        $(".console").console();
    </script>
</body>
</html>
