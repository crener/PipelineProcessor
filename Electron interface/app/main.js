
const electron = require('electron');
const exec = require('child_process');
const app = electron.app;

const BrowserWindow = electron.BrowserWindow;
require('electron-debug')({ enabled: true});
const path = require('path');
const url = require('url');

const exeName = "PipelineProcessor2.exe";
var server;

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
var mainWindow;

function createWindow () {
    // Create the browser window.
    mainWindow = new BrowserWindow({width: 1200, height: 600, minWidth: 970, minHeight: 530});

    mainWindow.loadURL(url.format({
        pathname: path.join(__dirname, 'windows/liteGraphEditor.html'),
        protocol: 'file:',
        slashes: true
    }));

    mainWindow.on('closed', function () {
        mainWindow = null
    });

    server = exec.execFile(exeName, function(err, data) {
        console.log(err);
        console.log(data.toString());
    });
}

function stopServer(){
    server.kill();
}

app.on('ready', createWindow);

// Quit when all windows are closed.
app.on('window-all-closed', function () {
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if (process.platform !== 'darwin') {
        app.quit()
    }
    stopServer();
});

app.on('activate', function () {
    if (mainWindow === null) {
        createWindow()
    }
});

app.on('quit', stopServer);
