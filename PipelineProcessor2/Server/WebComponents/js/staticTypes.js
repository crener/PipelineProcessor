LiteGraph.registerRawNodeType("Special/StartLoop", "Loop Start", "Start of loop",
    [{name: "1", type: "@1"}, {name: "2", type: "@2"}],
    [{name: "Link", type: "~Special/EndLoop"}, {name: "Count", type: "int"}, {name: "1", type: "@0"},
        {name: "2", type: "@1"}]);

LiteGraph.registerRawNodeType("Special/EndLoop", "Loop End", "End of loop",
    [{name: "Link", type: "~Special/StartLoop"}, {name: "Done", type: "bool"},
        {name: "1", type: "@0"}, {name: "2", type: "@1"}],
    [{name: "1", type: "@2"}, {name: "2", type: "@3"}]);

// LiteGraph.registerRawNodeType("Special/LoopLimit", "Loop Limiter", "Limits total loops",
//     [], [{name: "done", type: "bool"}]);

LiteGraph.registerRawNodeType("Special/Sync", "Sync", "Sync",
    [{name: "1", type: "@0"}, {name: "2", type: "@1[]"}],
    [{name: "1", type: "@0"}, {name: "2", type: "@1[]"}]);