(function ($) {
    $.fn.treeview = function (a, b) {
        return this.each(function () {
            var $this = $(this);

            var selectedPath = function () {
                return $('.selected-path', $this).val();
            };

            var pathDelimiter = function () {
                return $(".path-delimiter", $this).val();
            }

            var isLeaf = function (node) {
                return node.hasClass('leaf');
            };

            var isCollapsed = function (node) {
                return node.hasClass('collapsed');
            };

            var isExpanded = function (node) {
                return node.hasClass('expanded');
            };

            var findNode = function (path, callback) {
                var splitter = path.split(pathDelimiter());
                var list = $('ul.root', $this);
                var node = null;
                for (x = 0; x < splitter.length; x++) {
                    var id = splitter[x];
                    node = list.children('li[data-id="' + id + '"]');
                    if (typeof callback == 'function')
                        callback(node);
                    list = node.children('ul');
                }
                return node;
            }

            var getParentNode = function (node) {
                return node.closest('ul').closest('li');
            };

            var collapseNode = function (node) {
                node.removeClass('expanded').addClass('collapsed');
                node.find('li').removeClass('expanded').addClass('collapsed');
            };

            var expandNode = function (node) {
                node.removeClass('collapsed').addClass('expanded');
            };

            var expandPath = function (path) {
                return findNode(path, expandNode);
            };

            var selectPath = function (path) {
                $('li', $this).removeClass('selected');
                node = findNode(path);
                node.addClass('selected');
                $('.selected-path', $this).val(path);
            };

            var collapseAll = function(){
                $('li', $this).removeClass('expanded').addClass('collapsed');
            }

            if (a == 'select' && typeof b != 'undefined') {
                selectPath(b);
            }
            else if (a == 'expand' && typeof b != 'undefined') {
                collapseAll();
                expandPath(b);
            }
            else {

                var options = $.extend({}, { 'autoscroll': false }, a);

                expandPath(selectedPath());
                selectPath(selectedPath());

                $this.on('click', '.node-text .node-text-clickarea', function (event) {
                    var node = $(this).closest('li.node');
                    if (isCollapsed(node)) {
                        node.closest('ul').find('li.node').each(function () {
                            if (!isLeaf($(this))) collapseNode($(this));
                        });
                        expandNode(node);
                        if (options.autoscroll)
                            window.scrollTo(0, $(this).offset().top);
                    }
                    else if (isExpanded(node))
                        collapseNode(node);
                });
            }
        });
    }
}(jQuery));