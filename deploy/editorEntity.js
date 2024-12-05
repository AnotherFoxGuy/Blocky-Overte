(function () {
    this.channelName = "com.anotherfoxguy.blockly-editor.updateContent"
    var _entity = this;

    Script.include([
        "./xmldom/xmldom.js",
        "./blockly/blockly_compressed.js",
        "./blockly/blocks_compressed.js",
        "./blockly/javascript_compressed.js",
        "./overte.js",
    ]);

    Blockly.utils.xml.injectDependencies(DOMParser.DOMParser);

    this.headlessWorkspace = new Blockly.Workspace();

    this.updateWorkspace = (id, event) => {
        if (event == null)
            return;

        let updateEvent = Blockly.Events.fromJson(event, this.headlessWorkspace);
        updateEvent.run(true);

        // var message = {
        //     id: id,
        //     entityID: _entity.entityID,
        //     method: "blocklyEditor.sendUpdateWorkspaceMessage",
        //     event: value
        // };
        // //print("Sending message: " + JSON.stringify(message));
        // Messages.sendMessage(_entity.channelName, JSON.stringify(message));

        var message = {
            id: id,
            entityID: _entity.entityID,
            method: "blocklyEditor.sendLoadWorkspaceMessage",
            data: Blockly.serialization.workspaces.save(this.headlessWorkspace)
        };
        //print("Sending message: " + JSON.stringify(message));
        Messages.sendMessage(_entity.channelName, JSON.stringify(message));
    }

    this.sendLoadWorkspaceMessage = (value) => {
        var message = {
            entityID: _entity.entityID,
            method: "blocklyEditor.sendLoadWorkspaceMessage",
            data: value
        };
        //print("Sending message: " + JSON.stringify(message));
        Messages.sendMessage(_entity.channelName, JSON.stringify(message));
    }

    this.getProjectnameFromDescription = () => {
        var entityProps = Entities.getEntityProperties(_entity.entityID, ["description"]);
        _entity.projectName = entityProps.description;
        return _entity.projectName;
    }

    this.loadFile = () => {
        Assets.getAsset(
            {
                url: `/blockly/${_entity.getProjectnameFromDescription()}.js`,
                responseType: "text"
            },
            (error, result) => {
                if (error) {
                    print("ERROR: Data not downloaded");
                } else {
                    //print("Data: " + result.response);
                    if (result.response == null)
                        return;
                    let workspace = result.response.slice(2, result.response.indexOf("\n"));
                    // print("Data: " + workspace);
                    _entity.sendLoadWorkspaceMessage(workspace);
                }
            }
        );
    }

    this.saveFile = (contents) => {
        Assets.putAsset(
            {
                data: String(contents),
                path: `/blockly/${_entity.getProjectnameFromDescription()}.js`
            },
            (error, result) => {
                if (error) {
                    print("ERROR: Data not uploaded or mapping not set");
                } else {
                    print("URL: " + result.url);
                }
            }
        );
    }

    this.sendUpdateProjectname = () => {
        var projectName = _entity.getProjectnameFromDescription();
        var webMessage = {
            app: "blocklyEditor",
            method: "blocklyEditor.updateProjectname",
            projectName: projectName
        };
        Entities.emitScriptEvent(_entity.entityID, JSON.stringify(webMessage));
        //print("Web message sent");
    };

    this.onWebEventReceived = (entityID, message) => {
        //print("Web event received: " + entityID + that.entityID);
        if (entityID !== _entity.entityID) {
            return;
        }
        //print(message);
        try {
            message = JSON.parse(message.toString());
        } catch (error) {
            console.log("Couldn't parse web event message: " + error);
            return;
        }
        if (message.app != "blocklyEditor") {
            return;
        }
        switch (message.method) {
            case "blocklyEditor.onChangeHandler":
                //print("New script value: " + message.data);
                _entity.updateWorkspace(message.id, message.event);
                break;
            case "blocklyEditor.saveButtonClick":
                // print("Save button pressed: " + message.data);
                _entity.saveFile(message.data);
                break;
            case "blocklyEditor.loadButtonClick":
                // print("Load button pressed");
                _entity.loadFile();
                break;
            case "blocklyEditor.onLoadEvent":
                print("onLoadEvent received");
                _entity.sendUpdateProjectname();
                break;
        }
    }

    this.onMessageReceived = (channel, message, sender, localOnly) => {
        // console.log(`AAAAAAAAAAAAa ${channel} ${message} ${sender} ${localOnly}`);
        if (channel !== _entity.channelName) {
            return;
        }
        try {
            message = JSON.parse(message);
        } catch (error) {
            console.log("Couldn't parse message: " + error);
            return;
        }
        if (message.entityID != _entity.entityID) {
            return;
        }
        // print("Message received:");
        // print("- channel: " + channel);
        // print("- message: " + JSON.stringify(message));
        // print("- sender: " + sender);
        switch (message.method) {
            case "blocklyEditor.sendUpdateWorkspaceMessage":
                var webMessage = {
                    id: message.id,
                    app: "blocklyEditor",
                    method: "blocklyEditor.updateWorkspace",
                    event: message.event
                };
                Entities.emitScriptEvent(_entity.entityID, JSON.stringify(webMessage));
                //print("Web message sent");
                break;
            case "blocklyEditor.sendLoadWorkspaceMessage":
                var webMessage = {
                    app: "blocklyEditor",
                    method: "blocklyEditor.loadWorkspace",
                    data: message.data
                };
                Entities.emitScriptEvent(_entity.entityID, JSON.stringify(webMessage));
                //print("Web message sent");
                break;
        }
    }

    this.updateEvent = () => {
        _entity.sendUpdateProjectname();
    }

    this.preload = (entityID) => {
        _entity.entityID = entityID;
        //print("Entity ID: " + that.entityID);
        Entities.webEventReceived.connect(_entity.onWebEventReceived);
        Messages.subscribe(_entity.channelName);
        Messages.messageReceived.connect(_entity.onMessageReceived);
        Script.scriptEnding.connect(() => {
            Messages.messageReceived.disconnect(_entity.onMessageReceived);
            Entities.webEventReceived.disconnect(_entity.onWebEventReceived);
            Messages.unsubscribe(_entity.channelName);
            Script.clearInterval(_entity.updateTimer);
        });
        _entity.sendUpdateProjectname();
        // _entity.updateTimer = Script.setInterval(_entity.updateEvent, 1000);
    };
});


