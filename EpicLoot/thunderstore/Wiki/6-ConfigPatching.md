## What is this?

In Epic Loot v0.9.3 we added a new system to allow users to patch individual parts of the config files. This eliminates the need to overwrite files entirely for your particular mod as well as creates an easily distributable way to add things to Epic Loot without having to re-do them every time the base config files change.

Our hope is that mod and modpack authors can use this system to simply and easily create amazing custom content and configurations for Epic Loot without as much hassle.

As of 0.9.8, the current list of built-in config files is:
* abilities.json
* adventuredata.json
* enchantcosts.json
* iteminfo.json
* itemnames.json
* legendaries.json
* loottables.json
* magiceffects.json
* materialconversions.json
* recipes.json
* translations.json

Any of these files can be patched by the patching system.

## Add Patch Files

All Epic Loot config patches live in "patch files", which are json files placed in a special config folder located at Valheim\BepInEx\config\EpicLoot\patches

![Screenshot 2023-01-22 061059](https://user-images.githubusercontent.com/3331569/213920346-62d125ae-fd38-4efc-886f-68c286ef52a4.png)
![image](https://user-images.githubusercontent.com/3331569/213920363-dbf24991-9ba3-4d0c-9539-7678495dc916.png)

### Patch Folder Structure

Within the "patches" folder, there can be any number of sub-folders and it is recommended that your patch files are put in folders with distinct and unique names related to the author or modpack that created them. For example, if Epic Loot were to release a set of optional patches that changed the loottables just for Meadows creatures, we might install those patch files in folder structure something like: `<BepInEx Config Directory>\EpicLoot\patches\randyknapp\meadowsoverhaul`. That way, if we release a similar set of patches for a different biome, we could install the new patches in a folder of a different name, but within the `randyknapp` folder.

### Patch File Names

There are two ways to name your patch files:
1. Name them the exact same name as the file that you want to patch
1. Name them whatever you want, and specify which config file they patch within the file itself

The easiest option is #1. Creating a file called `loottables.json` or `adventuredata.json` in your patch folder will set it up so that all patches in those files will automatically be applied to their respective config files.

However, if you want to name your patch files easy to understand or memorable names for organizational purposes that is fine. Inside the file you'll be able to specify which config file the patches are targeting.

## Setup Patch File

Each patch file is a json file, and therefor must be edited with a text editor. If you don't already have an IDE for modding (like Visual Studio, Rider, or VS Code), I recommend Sublime Text or Notepad++ for editing json files.

> Epic Loot's json files are technically cjson, which is an extension of json that allows C++ style comments. Some text editors do not like that when doing syntax highlighting for json files and we're sorry about that.

As a json file, the main part of the file must be an object. And the main object type for our patch files is the PatchFile class (see [FilePatching.cs](https://github.com/RandyKnapp/ValheimMods/blob/main/EpicLoot/Patching/FilePatching.cs)).

Here is an example of a patch file (without any patches yet) with all the fields filled in:

    {
      "TargetFile": "adventuredata.json", 
      "Author": "RandyKnapp",
      "Priority": 500,
      "RequireAll": true, 
      "Patches": []
    }

### TargetFile (string)
If your file is not named exactly the same as one of the base config files, this field is required. If your file _is_ named the same as one of the base config files and you specify a _different_ config file in this field, the patch will use the one specified in the TargetFile field, but it will spit out a warning at you.

The value of this field will be used as the default value for the TargetFile field of each patch (see next section) if it is not specified.

### Author (string)
Purely for documentation purpose, this field can be any string that identifies the individual or team that published the patch.

### Priority (int)
This Priority field indicates the order in which patches will be executed. It must be a positive integer between 0 and 1000 (inclusive). 1000 is the highest priority and will be applied first, 0 is the lowest and will be applied last.

The value of this field is used as the default value for the Priority field of each patch if it is not specified. The default value for this field if unspecified is `500`.

### RequireAll (bool)
The RequireAll field is a boolean (true/false) field indicating if all the patches in this file are "required". A "required" patch means that if the patch's Path (see the Setup Patches section) does not select any json tokens from the base config file, an error will be thrown. Sometimes it is valuable if you are optionally patching someone else's patches that may or may not be loaded by the player to have it simply ignore failed patches if the item they are trying to patch doesn't exist.

If set to true, all patches will have their Required field overwritten to true. If set to false, each patch can set its own Required field (which defaults to false if not provided). The default value of RequireAll if unspecified is `false`.

### Patches (array of Patch objects)
This array holds all the patches that you want to apply. It will be a comma separated list of objects, each containing the patch data.

    "Patches": [
      {
        // patch data...
      },
      {
        // patch data...
      }
    ]

## Setup Patches

Now that you've set up the file properties, you can go about creating any number of patches. Each patch has several fields which will be explained in detail below. Most fields are optional and will be set to the default values if you omit them in your patch file.

    {
      "Priority": 500,
      "TargetFile": "adventuredata.json",
      "Required": true,
      "Path": "$.SecretStash.OtherItems[?(@.CoinsCost < 300)]",
      "Action": "Add",
      "PropertyName": "ForestTokenCost",
      "Value": 10
    },

### Priority (int)
Like the Priority of the file object, each individual patch can set its own priority that will override the default priority set in the file. This  field indicates the order in which patches will be executed. It must be a positive integer between 0 and 1000 (inclusive). 1000 is the highest priority and will be applied first, 0 is the lowest and will be applied last.

If unspecified, the default value for this field is whatever is set in the Priority field of the file object.

### TargetFile (string)
It is perfectly okay to have a single file with patches being applied to many different base config files. If a TargetFile field is not specified in the file object itself, then each patch must specify which config file they wish to patch.

An error will be thrown and the patch will be skipped if both the file and the patch are missing the TargetFile field, or if the file specified is not one of the files listed at the beginning of this article.

### Required (bool)
The Required field is a boolean (true/false) field indicating if this patch is "required". A "required" patch means that if the patch's Path (see below) does not select any json tokens from the base config file, an error will be thrown. Sometimes it is valuable if you are optionally patching someone else's patches that may or may not be loaded by the player to have it simply ignore failed patches if the item they are trying to patch doesn't exist.

If set to true, this patch will be required. If omitted or set to false, this patch will use the value of the RequireAll field of the file object.

### Path (JsonPath string)
This complex string tells the patching system exactly which part or parts of the target file you want to change. It uses the JsonPath syntax by Stefan Goessner, which can be found in brief here: [JsonPath Specification](https://goessner.net/articles/JsonPath/)

EpicLoot is now using the Newtonsoft Json Parser for .NET. You can find examples of how JsonPath is used to select json tokens here: [Json.NET Docs - Querying JSON with JsonPath](https://www.newtonsoft.com/json/help/html/QueryJsonSelectToken.htm)

This patch can resolve to zero, one, or many tokens in the file. If the patch is Required (see below), and the path resolves to zero selected tokens, then an error will be displayed in the BepInEx console.

Once the path resolves to some number of tokens, the patch Action (see below) will be taken against each of those tokens.

> In the example above:
> 
>     "Path": "$.SecretStash.OtherItems[?(@.CoinsCost < 300)]",
> 
> the path will select the "SecretStash" object at the root of the adventuredata.json file, then select its child object called "OtherItems", which is an array, and then it will select every object in that array that has a property "CoinsCost" of less than 300. Remember, this can result in selecting multiple tokens.

### Action (string, one of an enumeration)
The Action field can be set to one of the following values, and each one does something different when the patch is applied to the selected token.

    Action         What it does
    -----------------------------------------------------------------------------------------------------------------
    None           Do nothing.
    
    Add            Add the provided value to the selected object with the provided property name, if the property 
                   already exists, it's value is overwritten.
                     Must be specified: Value, PropertyName
    
    Overwrite      Replace the selected token's value with the provided value.
                     Must be specified: Value
    
    Remove         Remove the selected token from the array or object.
    
    Append         Append the provided value to the end of the selected array.
                     Must be specified: Value
    
    AppendAll      Append the provided array value to the end of the selected array.
                     Must be specified: Value (must be an array)
    
    InsertBefore   Insert the provided value into the array containing the selected token, before the token.
                     Must be specified: Value
    
    InsertAfter    Insert the provided value into the array containing the selected token, after the token.
                     Must be specified: Value
    
    RemoveAll      Remove all elements of an array or all properties of an object.

### PropertyName (string)
This optional field specifies the name of the property when patching using the Add action from above. It is only needed when the patch is using the Add action.

### Value (any valid json)
The Value field of the patch can contain any valid json value you want. The entirety of this value will be added, used to overwrite, inserted, or appended to the token(s) selected by this patch's Path. When using AppendAll, Value must be an Array ("Value" : []).

## Happy patching!
If you have any questions, please join the RandyKnapp Modding Discord and ask: [RandyKnapp's Mod Community](https://discord.gg/randyknappmods)