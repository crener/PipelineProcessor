# Generic Pipeline Processor

![Electron front end image](https://raw.github.com/crener/PipelineProcessor/img/nested.png)

This is a generic processing pipeline Built to maximize CPU performance by maximizing parallelism (using both data and task based parallelism).
Each node (as seen above) represents an operation on data and can be made by anyone by simply implementing an interface and dropping the resulting dll into a folder. Data 'flows' through the inputs and outputs of nodes until it reaches the end where it is written to disk (or whatever the output node determines).

## Have an issue, found a Bug, confused?
if you have any questions about this open an [issue](https://github.com/crener/PipelineProcessor/issues/new) and I'll figure out how to help!


## Todo
 - Context aware node recommendations when dragging a connection into empty space
 - Limit connectable nodes by compatible slots
 - Transfer Electron application to built web app hosted from processor (partly complete)
 - Add python support (and more languages depending on interest/needs)

 Any ideas about what to add? [Make an issue](https://github.com/crener/PipelineProcessor/issues/new)
