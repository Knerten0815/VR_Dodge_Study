var folder = null;

try { 

    activeDocument; 

    folder = Folder.selectDialog("Select your folder", "C:\\");

    }

catch(e)

    {

    alert("No activeDocument", "");

    }

    

if (folder) 

    {

    refresh();

        

    place_files(folder);

	deleteAllEmptyLayerSets(activeDocument)

    alert("Done!", "");     

    }        

function place_files(folder)  

    {  

    try  

        {  

        var layer = activeDocument.activeLayer;

    

        add_group(folder.name);

        add_layer(folder.name);

        layer = activeDocument.activeLayer;

        var f = folder.getFiles();  

        for (var i = 0; i < f.length; i++)  
    {  
    if (f[i] instanceof Folder)   
        {
        place_files(f[i]);  

        activeDocument.activeLayer = layer;
        }
    else  
        if (f[i].name.match(/\.(psd|jpg|tif|png)$/i))
            {
            place_file(f[i]);
            }
    }  

        layer.remove();

        f = null;  

        }  

    catch (e) { alert(e); }  

    }  

function add_group(name)

    {

    try { 

        var d = new ActionDescriptor();

        var r = new ActionReference();

        r.putClass(stringIDToTypeID("layerSection"));

        d.putReference(stringIDToTypeID("null"), r);

        var d1 = new ActionDescriptor();

        d1.putString(stringIDToTypeID("name"), name);

        d.putObject(stringIDToTypeID("using"), stringIDToTypeID("layerSection"), d1);

        executeAction(stringIDToTypeID("make"), d, DialogModes.NO);

        } 

    catch (e) { alert(e); throw(e); } 

    }

function add_layer(name)

    {

    try {

        var d = new ActionDescriptor();

        var r = new ActionReference();

        r.putClass(stringIDToTypeID("layer"));

        d.putReference(stringIDToTypeID("null"), r);

        var d1 = new ActionDescriptor();

        d1.putString(stringIDToTypeID("name"), name);

        d.putObject(stringIDToTypeID("using"), stringIDToTypeID("layer"), d1);

        executeAction(stringIDToTypeID("make"), d, DialogModes.NO);

        }

    catch (e) { alert(e); throw(e); }

    }

function place_file(file)

    {

    try {

        var d = new ActionDescriptor();

        d.putPath(stringIDToTypeID("file"), file);

        var doc = executeAction(stringIDToTypeID("openViewlessDocument"), d, DialogModes.NO).getData(stringIDToTypeID("document"));

        var d = new ActionDescriptor();

        var l = new ActionList();

        l.putPath(file);

        d.putList(stringIDToTypeID("fileList"), l);

        var l = new ActionList();

        l.putData(doc);

        d.putList(stringIDToTypeID("viewlessDoc"), l);

        executeAction(stringIDToTypeID("addLayerFromViewlessDoc"), d, DialogModes.NO);

        }

    catch (e) { alert(e); throw(e); }

    }

	

function deleteAllEmptyLayerSets(o){
    try{
        var bEmpty = true;
        for(var i = o.layerSets.length-1; 0 <= i; i--) {
            var layerSet = o.layerSets[i];
            if((layerSet.allLocked)||(0!=layerSet.linkedLayers.length)){
                continue; // skip locked or linked
            }
            if(deleteAllEmptyLayerSets(layerSet)){ // empty
                layerSet.remove(); // delete
            } else  bEmpty = false;      // not empty
        }
        if(0 != o.artLayers.length) bEmpty = false; // not empty
    }catch(e){
        ; // do nothing
    }
    return bEmpty;
}