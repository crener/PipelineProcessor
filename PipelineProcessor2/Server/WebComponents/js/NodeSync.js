var rp = require('request-promise');
var fetchingNodes = false;
process.env.no_proxy = "localhost";

var address = "localhost";

var updateNodes = function () {
    if (fetchingNodes) return;
    fetchingNodes = true;

    rp("http://" + address + ":9980/nodes")
        .then(function (result) {
            fetchingNodes = false;
            var cppNodes = JSON.parse(result);

            if (!(cppNodes instanceof Array)) {
                console.error("Cpp Active Nodes not valid");
                return;
            }

            cppNodes.forEach(function (entry) {
                //ensure no duplicates
                if (LiteGraph.registered_node_types[entry.type] === undefined) {
                    LiteGraph.registerRawNodeType(
                        entry.type, entry.title,
                        entry.desc, entry.input,
                        entry.output, entry.useValue,
                        entry.defaultValue);
                }
            })
        })
        .catch(function (error) {
            console.error("Error getting active nodes: " + error);
            fetchingNodes = false;
        })
};

updateNodes();
setInterval(updateNodes, 15000);

var submittingGraph = false;
var updateNodeGraph = function () {
    if (submittingGraph) return;
    submittingGraph = true;
    var defaultData = graph.serialize();

    var returnData = {};
    returnData.links = defaultData.links;
    returnData.nodes = defaultData.nodes;
    returnData.input = document.getElementById("inputDir").value;
    returnData.output = document.getElementById("outputDir").value;


    var options = {
        method: 'POST',
        uri: "http://" + address + ":9980/graph/update",
        body: returnData,
        json: true // Automatically stringifies the body to JSON
    };

    rp(options).then(
        function (response) {
            submittingGraph = false;
        })
        .catch(function (error) {
            console.error("Error submitting nodes: " + error);
            submittingGraph = false;
        })
};