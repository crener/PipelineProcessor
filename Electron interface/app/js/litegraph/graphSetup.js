var app = require('electron').remote;
var dialog = app.dialog;
var fs = require('fs');

//Creates an interface to access extra features from a graph (like play, stop, live, etc)
function Editor(container_id, options) {
    //fill container
    var html = "<div class='header'><div class='tools tools-left'></div>";
    html += "<div class=\"tools\" style=\"margin-right: 70px; margin-top: 7px; font-family:Tahoma; color: #AAA;\">Input: <input id=\"inputDir\"> Output: <input id=\"outputDir\"></div>";
    html += "<div class='tools tools-right'></div></div>";
    html += "<div class='content'><div class='editor-area'><canvas class='graphcanvas' width='100%' height='400' tabindex=10></canvas></div></div>";
    html += "<div class='footer'><div class='tools tools-left'></div><div class='tools tools-right'></div></div>";

    var root = document.createElement("div");
    this.root = root;
    root.className = "litegraph-editor";
    root.innerHTML = html;

    this.tools = root.querySelector(".tools");

    var canvas = root.querySelector(".graphcanvas");

    //create graph
    var graph = this.graph = new LGraph();
    var graphcanvas = this.graphcanvas = new LGraphCanvas(canvas, graph);
    graphcanvas.background_image = "../js/litegraph/imgs/grid.png";
    graph.onAfterExecute = function () {
        graphcanvas.draw(true)
    };

    //add stuff
    this.addToolsButton("loadsession_button", "Load", "../js/litegraph/imgs/icon-load.png", this.onLoadButton.bind(this), ".tools-left");
    this.addToolsButton("savesession_button", "Save", "../js/litegraph/imgs/icon-save.png", this.onSaveButton.bind(this), ".tools-left");
    this.addToolsButton("play_button", "", "../js/litegraph/imgs/icon-play.png", this.onPlayButton.bind(this), ".tools-right");

    this.tools.innerHTML += " Value: <input id=\"nodeValue\" disabled>";

    this.addMiniWindow(300, 200);

    //append to DOM
    var parent = document.getElementById(container_id);
    if (parent)
        parent.appendChild(root);

    document.getElementById("loadsession_button").addEventListener("click", this.onLoadButton);
    document.getElementById("savesession_button").addEventListener("click", this.onSaveButton);
    document.getElementById("play_button").addEventListener("click", this.onPlayButton);

    graphcanvas.resize();
    graphcanvas.draw(true, true);
}

Editor.prototype.addToolsButton = function (id, name, icon_url, callback, container) {
    if (!container)
        container = ".tools";

    var button = this.createButton(name, icon_url);
    button.id = id;
    if(container === ".tools-left"){
        button.style = "margin-top: -6px;"
    }
    button.addEventListener("click", callback);

    this.root.querySelector(container).appendChild(button);
}


Editor.prototype.createButton = function (name, icon_url) {
    var button = document.createElement("button");
    if (icon_url)
        button.innerHTML = "<img src='" + icon_url + "'/> ";
    button.innerHTML += name;
    return button;
}

Editor.prototype.onLoadButton = function () {
    var options = {
        title: "Load project",
        filters: [
            {name: "Project", extensions: ['proj']},
            {name: 'All Files', extensions: ['*']}
        ]
    };

    dialog.showOpenDialog(options, function (fileNames) {
        // fileNames is an array that contains all the selected
        if (fileNames === undefined) {
            console.log("No file selected");
            return;
        }

        var filepath = fileNames[0];

        fs.readFile(filepath, 'utf-8', function (err, data) {
            if (err) {
                alert("An error occurred reading the file :" + err.message);
                return;
            }

            var load = JSON.parse(data);
            graph.configure(load.nodes);
            if(load.in) document.getElementById("inputDir").value = load.in;
            if(load.out) document.getElementById("outputDir").value = load.out;
        });
    });
};

Editor.prototype.onSaveButton = function () {

    var rawData = {
        nodes : graph.serialize(),
        in : document.getElementById("inputDir").value,
        out : document.getElementById("outputDir").value
    };
    var data = JSON.stringify(rawData);
    var options = {
        title: "Save project",
        filters: [
            {name: "Project", extensions: ['proj']},
            {name: 'All Files', extensions: ['*']}
        ]
    };

    dialog.showSaveDialog(options, function (file) {
        if (file === undefined) {
            console.log("You didn't save the file");
            return;
        }

        fs.writeFile(file, data, function (err) {
            if (err) {
                alert("An error occurred creating the file " + err.message)
            }
        });
    });
};

Editor.prototype.onPlayButton = function () {
    updateNodeGraph();
}

Editor.prototype.addMiniWindow = function (w, h) {
    var miniwindow = document.createElement("div");
    miniwindow.className = "litegraph miniwindow";
    miniwindow.innerHTML = "<canvas class='graphcanvas' width='" + w + "' height='" + h + "' tabindex=10></canvas>";
    var canvas = miniwindow.querySelector("canvas");

    var graphcanvas = new LGraphCanvas(canvas, this.graph);
    graphcanvas.background_image = "../js/litegraph/imgs/grid.png";
    graphcanvas.scale = 0.25;
    graphcanvas.allow_dragnodes = false;
    graphcanvas.allow_interaction = false;
    this.miniwindow_graphcanvas = graphcanvas;
    graphcanvas.onClear = function () {
        graphcanvas.scale = 0.25;
        graphcanvas.allow_dragnodes = false;
        graphcanvas.allow_interaction = false;
    };

    miniwindow.style.position = "absolute";
    miniwindow.style.top = "4px";
    miniwindow.style.right = "4px";

    var close_button = document.createElement("div");
    close_button.className = "corner-button";
    close_button.innerHTML = "X";
    close_button.addEventListener("click", function (e) {
        graphcanvas.setGraph(null);
        miniwindow.parentNode.removeChild(miniwindow);
    });
    miniwindow.appendChild(close_button);

    this.root.querySelector(".content").appendChild(miniwindow);
}

LiteGraph.Editor = Editor;