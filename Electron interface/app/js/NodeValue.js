var settings = global.Settings = {

    nodeSelected: function () {
        var node = this;
        var value = document.getElementById("nodeValue");

        if (!node.useValue) {
            value.setAttribute("disabled", "");
            value.value = "";
        }
        else {
            value.removeAttribute("disabled");
            value.value = node.value;
        }
    },

    nodeDeselected: function () {
        var node = this;
        var value = document.getElementById("nodeValue");

        if (node.useValue) {
            value.removeAttribute("disabled");
            node.value = value.value;
        }

        value.setAttribute("disabled", "");
        value.value = "";
    }
};

LiteGraph.addNodeMethod("onSelected", settings.nodeSelected);
LiteGraph.addNodeMethod("onDeselected", settings.nodeDeselected);
LiteGraph.addNodeMethod("onRemoved", settings.nodeDeselected);
