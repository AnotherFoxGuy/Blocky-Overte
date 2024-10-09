﻿using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Overte;
using Scriban;
using System.Security.Cryptography;
using System.Text;

var json = File.ReadAllText("hifiJSDoc.json");
json = json.Replace("(0)", "");
var data = JsonConvert.DeserializeObject<List<HifiJsDoc>>(json);


// Templates
var toolbox_template = Template.Parse(File.ReadAllText("./default_toolbox.js.sbn"), "./default_toolbox.js.sbn");
var class_template = Template.Parse(File.ReadAllText("./classTemplate.js.sbn"), "./classTemplate.js.sbn");
var function_template = Template.Parse(File.ReadAllText("./functionTemplate.js.sbn"), "./functionTemplate.js.sbn");
var signal_template = Template.Parse(File.ReadAllText("./signalTemplate.js.sbn"), "./signalTemplate.js.sbn");
var typedef_template = Template.Parse(File.ReadAllText("./typedefTemplate.js.sbn"), "./typedefTemplate.js.sbn");
var namespace_template = Template.Parse(File.ReadAllText("./namespaceTemplate.js.sbn"), "./namespaceTemplate.js.sbn");

testTemplate(toolbox_template);
testTemplate(class_template);
testTemplate(function_template);
testTemplate(signal_template);
testTemplate(typedef_template);
testTemplate(namespace_template);

// var test = data.Where(d => d.Memberof == "Agent" || d.Name == "Agent");

var outp = "";

var toolboxcont = new Dictionary<string, List<string>>();
var color_cache = new Dictionary<string, string>();
var block_cache = new List<string>();

foreach (var t in data.Where(d => d.Deprecated == null)) // .Where(d => d.Memberof == "MyAvatar")
{
    switch (t.Kind)
    {
        case Kind.Namespace:
            Console.WriteLine($"Namespace {t.Longname}");
            outp += generateNamespaceBlock(t);
            break;
        case Kind.Function:
            Console.WriteLine($"Function {t.Longname}");
            outp += generateFunctionBlock(t);
            break;
        case Kind.Signal:
            Console.WriteLine($"Signal {t.Longname}");
            outp += generateSignalBlock(t);
            break;
        case Kind.Typedef:
            Console.WriteLine($"Typedef {t.Longname}");
            outp += generateTypedefBlock(t);
            break;
            // default:
            //     break;
    }

}

toolboxcont = toolboxcont.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
var rend = toolbox_template.Render(new { Toolbox = toolboxcont });
File.WriteAllText("./deploy/overte.js", outp + rend);

string generateNamespaceBlock(HifiJsDoc data)
{
    if (block_cache.Contains($"{data.Memberof}{data.Name}"))
        return "";
    block_cache.Add($"{data.Memberof}{data.Name}");

    var i = 0;
    var block_name = getBlockName(data);
    var outp = "";

    if (data.Properties == null)
        return outp;

    foreach (var prop in data.Properties)
    {
        if (prop.Type == null || prop.Name == null)
            continue;

        var desc = prop.Description?.Replace("'", "\\'") ?? "";
        desc = Regex.Replace(desc, @"\t|\n|\r", "");

        var name = $"{data.Name}.{prop.Name}";

        var template_data = new
        {
            Jsfunction = name,
            Blockname = $"{data.Name}_{prop.Name}",
            Description = desc,
            Url = $"https://apidocs.overte.org/{name.Replace(".", ".html#.")}",
            Color = catColor(data.Memberof ?? data.Name)
        };
        addToToolbox(data, $"{data.Name}_{prop.Name}");
        outp += namespace_template.Render(template_data);
    }

    return outp;
}


string generateTypedefBlock(HifiJsDoc data)
{
    if (block_cache.Contains($"{data.Memberof}{data.Name}"))
        return "";
    block_cache.Add($"{data.Memberof}{data.Name}");

    var i = 0;
    var block_name = getBlockName(data);
    var properties = new List<object>();

    if (data.Properties == null || data.Type?.Names.First() != "object")
        return "";

    foreach (var prop in data.Properties)
    {
        if (prop.Type == null)
            continue;

        var parm_name = prop.Name ?? $"prop_{i++}";
        parm_name = Regex.Replace(parm_name, @"[^a-zA-Z0-9]", "");
        if (parm_name == "")
            parm_name = $"prop_{i++}";

        properties.Add(new
        {
            Name = parm_name,
            Description = prop.Description,
            Type = typeToJs(prop.Type)
        });
    }


    var desc = data.Description?.Replace("'", "\\'") ?? "";
    desc = Regex.Replace(desc, @"\t|\n|\r", "");

    var template_data = new
    {
        Jsfunction = data.Longname,
        Blockname = getBlockName(data),
        Description = desc,
        Properties = properties,
        Url = $"https://apidocs.overte.org/{data.Longname.Replace(".", ".html#.")}",
        Color = catColor(data.Memberof ?? data.Name)
    };

    addToToolbox(data);
    return typedef_template.Render(template_data);
}


string generateClassBlock(HifiJsDoc data)
{
    var desc = data.Description?.Replace("'", "\\'") ?? "";
    desc = Regex.Replace(desc, @"\t|\n|\r", "");

    var template_data = new
    {
        JsFunction = data.Longname,
        Blockname = getBlockName(data),
        Description = desc,
        // Codefields = string.Join(",", code_fields),
        Properties = data.Properties
    };

    return class_template.Render(template_data);
}

string generateSignalBlock(HifiJsDoc data)
{
    if (block_cache.Contains($"{data.Memberof}{data.Name}"))
        return "";
    block_cache.Add($"{data.Memberof}{data.Name}");

    var block_name = getBlockName(data);
    var parameters = new List<object>();

    if (data.Params != null)
    {
        foreach (var param in data.Params)
        {
            if (param.Type == null)
                continue;

            var parm_name = param.Name ?? "parameter";
            parm_name = parm_name.Replace("-", "");

            parameters.Add(new
            {
                Name = parm_name,
                Type = typeToJs(param.Type)
            });
        }
    }

    var desc = data.Description?.Replace("'", "\\'") ?? "";
    desc = Regex.Replace(desc, @"\t|\n|\r", "");

    var template_data = new
    {
        Jsfunction = data.Longname,
        Blockname = getBlockName(data),
        Description = desc,
        Parameters = parameters,
        Url = $"https://apidocs.overte.org/{data.Longname.Replace(".", ".html#.")}",
        Color = catColor(data.Memberof)
    };

    addToToolbox(data);
    return signal_template.Render(template_data);
}


string generateFunctionBlock(HifiJsDoc data)
{

    var parameters = new List<object>();

    if (data.Params != null)
    {
        foreach (var param in data.Params)
        {
            if (param.Type == null)
                continue;

            var parm_name = param.Name ?? "parameter";
            parm_name = parm_name.Replace("-", "");

            parameters.Add(new
            {
                Name = parm_name,
                Type = typeToJs(param.Type)
            });
        }
    }

    var returns = new List<string>();

    if (data.Returns != null)
    {
        foreach (var o in data.Returns)
        {
            if (o.Type == null)
                continue;
            returns.Add(typeToJs(o.Type));
        }
    }

    var block_name = getBlockName(data);

    var desc = data.Description?.Replace("'", "\\'") ?? "";
    desc = Regex.Replace(desc, @"\t|\n|\r", "");

    var template_data = new
    {
        Jsfunction = data.Longname,
        Blockname = getBlockName(data),
        Description = desc,
        Parameters = parameters,
        Returns = returns,
        Url = $"https://apidocs.overte.org/{data.Longname.Replace(".", ".html#.")}",
        Color = catColor(data.Memberof)
    };

    addToToolbox(data);
    return function_template.Render(template_data);
}

string getBlockName(HifiJsDoc data) => data.Longname.Replace('.', '_'); //.ToLower();

string typeToJs(TypeClass type)
{
    return type.Names.First() switch
    {
        "number" => "Number",
        "boolean" => "Boolean",
        "string" => "String",
        "*" => "Any",
        _ => type.Names.First(),
    };
}

string getBlocklyField(TypeClass type)
{
    return type.Names.First() switch
    {
        "number" => "Number",
        "boolean" => "Boolean",
        "string" => "String",
        "*" => "Any",
        _ => "",
    };
}


void testTemplate(Template t)
{
    if (t.HasErrors)
    {
        foreach (var error in t.Messages)
            Console.WriteLine(error);

        throw new Exception("Invalid template!");
    }
}

string normalizeVarName(string? inp) => inp == null ? "parameter" : inp.Replace("-", "");

string catColor(string? cat)
{
    if (cat == null)
        return "#111111";

    if (color_cache.TryGetValue(cat, out string? value))
        return value;

    var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(cat));
    var color = $"#{string.Join("", BitConverter.ToString(hashBytes).Replace("-", "").Take(6))}";
    color_cache.Add(cat, color);
    return color;
}

void addToToolbox(HifiJsDoc a, string? name = null)
{
    var cat = a.Memberof ?? a.Name;
    toolboxcont.TryAdd(cat, []);
    toolboxcont[cat].Add(name ?? getBlockName(a));
}