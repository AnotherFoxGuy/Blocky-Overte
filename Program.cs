using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Overte;
using Scriban;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var json = File.ReadAllText("hifiJSDoc.json");
json = json.Replace("(0)", "");
var jsdocData = JsonConvert.DeserializeObject<List<HifiJsDoc>>(json) ?? [];

// var test = data.Where(d => d.Memberof == "Agent" || d.Name == "Agent");

var blockCache = new List<string>();

var output = "";

var declarations = new List<TypeScriptDeclaration>();

var extras = "";

var template = Template.Parse(File.ReadAllText("./templates/declarationTemplate.ts.sbn"),
    "./templates/declarationTemplate.ts.sbn");

foreach (var t in jsdocData.Where(d => d.Deprecated == null)) // .Where(d => d.Memberof == "MyAvatar")
{
    switch (t.Kind)
    {
        case Kind.Namespace:
            Console.WriteLine($"Namespace {t.Longname}");
            generateNamespaceBlock(t);
            break;
        case Kind.Function:
            Console.WriteLine($"Function {t.Longname}");
            generateFunctionBlock(t);
            break;
        case Kind.Signal:
            Console.WriteLine($"Signal {t.Longname}");
            generateSignalBlock(t);
            break;
        case Kind.Typedef:
            Console.WriteLine($"Typedef {t.Longname}");
            generateTypedefBlock(t);
            break;
        case Kind.Class:
            Console.WriteLine($"Class {t.Longname}");
            generateClassBlock(t);
            break;
        // default:
        //     break;
    }
}

// toolboxcont = toolboxcont.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
// toolboxcont.Sort((x, y) => string.Compare(x.Name, y.Name));

foreach (var declaration in declarations)
{
    declaration.Variables = declaration.Variables.Distinct().ToList();
    declaration.Functions = declaration.Functions.Distinct().ToList();
    output += template.Render(declaration);
}

File.WriteAllText($"./deploy/overte.d.ts", output + extras);


void generateNamespaceBlock(HifiJsDoc data)
{
    if (blockCache.Contains($"Namespace{data.Memberof}{data.Name}"))
        return;
    blockCache.Add($"Namespace{data.Memberof}{data.Name}");

    if (data.Properties == null) return;
    var declaration = get(data);

    foreach (var prop in data.Properties)
    {
        if (declaration.Variables.Find(x => x.Name == prop.Name) != null)
            continue;

        var type = prop.Type == null ? "Any" : string.Join(" | ", prop.Type.Names.Select(cleanTypename));
        declaration.Variables.Add(new TypeScriptVariable()
        {
            Name = prop.Name,
            Description = CleanDescription(prop.Description),
            Type = type
        });
    }
}


void generateTypedefBlock(HifiJsDoc data)
{
    if (blockCache.Contains($"Typedef{data.Memberof}{data.Longname}"))
        return;
    blockCache.Add($"Typedef{data.Memberof}{data.Longname}");

    // if (data.Properties == null || data.Type?.Names.First() != "object")
    //     return "";

    var i = 0;

    if (data.Properties != null)
    {
        var BaseDeclaration = get(data);
        var sub = new TypeScriptDeclaration()
        {
            Name = data.Name.Replace("-", ""),
            Description = CleanDescription(data.Description)
        };

        foreach (var prop in data.Properties)
        {
            var parm_name = prop.Name == null || prop.Name == "?" ? $"arg_{i++}" : prop.Name;
            parm_name = parm_name.Replace("-", "").Replace(":", "").Replace(".", "");

            if (prop.Optional != null)
                parm_name += "?";

            var type = prop.Type == null ? "Any" : string.Join(" | ", prop.Type.Names.Select(cleanTypename));
            sub.Variables.Add(new TypeScriptVariable()
            {
                Name = parm_name.Replace("-", "").Replace(":", ""),
                Description = CleanDescription(prop.Description),
                Type = type
            });
        }

        BaseDeclaration.Subclasses.Add(sub);
    }
    else
    {
        var type = string.Join(" | ", data.Type.Names.Select(cleanTypename));
        if (data.Memberof != null)
            get(data).Extra += $"\texport type {data.Name.Replace("-", "")} = {type}\n";
        else
            extras += $"type {data.Name.Replace("-", "")} = {type}\n";
    }
}


void generateClassBlock(HifiJsDoc data)
{
    if (blockCache.Contains($"Class{data.Memberof}{data.Name}"))
        return;
    blockCache.Add($"Class{data.Memberof}{data.Name}");

    var declaration = get(data);

    // if (data.Properties == null || data.Type?.Names.First() != "object")
    //     return "";

    int i = 0;

    if (data.Properties != null)
    {
        foreach (var prop in data.Properties)
        {
            var parm_name = prop.Name == null || prop.Name == "?" ? $"arg_{i++}" : prop.Name;
            if (prop.Optional != null)
                parm_name += "?";

            // if (declaration.Variables.Find(x => x.Name == prop.Name) != null)
            //     continue;

            var type = prop.Type == null ? "Any" : string.Join(" | ", prop.Type.Names.Select(cleanTypename));
            declaration.Variables.Add(new TypeScriptVariable()
            {
                Name = parm_name,
                Description = CleanDescription(prop.Description),
                Type = type
            });
        }
    }
}

void generateSignalBlock(HifiJsDoc data)
{
    // if (blockCache.Contains($"Signal{data.Memberof}{data.Name}"))
    //     return "";
    // blockCache.Add($"Signal{data.Memberof}{data.Name}");
    //
    // var block_name = getBlockName(data);
    // var parameters = new List<object>();
    //
    // if (data.Params != null)
    // {
    //     foreach (var param in data.Params)
    //     {
    //         if (param.Type == null)
    //             continue;
    //
    //         var parm_name = param.Name ?? "parameter";
    //         parm_name = parm_name.Replace("-", "");
    //
    //         parameters.Add(new
    //         {
    //             Name = parm_name,
    //             Type = typeToJs(param.Type)
    //         });
    //     }
    // }
    //
    // var desc = data.Description?.Replace("'", "\\'") ?? "";
    // desc = Regex.Replace(desc, @"\t|\n|\r", "");
    //
    // var color = catColor(data.Memberof);
    //
    // var template_data = new
    // {
    //     Name = data.Name,
    //     Jsfunction = data.Longname,
    //     Blockname = getBlockName(data),
    //     Description = desc,
    //     Parameters = parameters,
    //     Url = $"https://apidocs.overte.org/{data.Longname.Replace(".", ".html#.")}",
    //     Color = color
    // };
    //
    // addToToolbox(data, color);
    // return signal_template.Render(template_data);
}


void generateFunctionBlock(HifiJsDoc data)
{
    var block_name = data.Name;

    if (blockCache.Contains($"Function{block_name}") && data.Params != null)
        foreach (var param in data.Params)
            block_name += $"{param.Name}";

    blockCache.Add($"Function{block_name}");

    var declaration = get(data);

    if (data.Params != null)
    {
        foreach (var param in data.Params)
        {
            if (param.Type == null)
                continue;

            var parm_name = param.Name ?? "parameter";
            parm_name = parm_name.Replace("-", "");
        }
    }

    var returns = new List<string>();

    if (data.Returns != null)
    {
        returns.AddRange(from o in data.Returns
            where o.Type != null
            select string.Join(" | ", o.Type.Names.Select(cleanTypename)));
    }

    var type = data.Returns == null ? "void" : string.Join(" | ", returns);
    declaration.Functions.Add(new TypeScriptFunction()
    {
        Name = data.Name,
        Description = CleanDescription(data.Description),
        Type = type
    });
}


// TypeScriptDeclaration get(HifiJsDoc data)
// {
//     var tsNamespace = typeScriptDeclarations.SingleOrDefault(x => x.Name == data.Longname);
//     if (tsNamespace != null) return tsNamespace;
//     
//     var type = data.Kind switch
//     {
//         Kind.Class => "class",
//         Kind.Namespace => "namespace",
//         _ => "",
//     };
//     tsNamespace = new TypeScriptDeclaration
//     {
//         Name = data.Longname,
//         Type = type,
//         Description = data.Description
//     };
//     typeScriptDeclarations.Add(tsNamespace);
//
//     return tsNamespace;
// }

string cleanTypename(string inp)
{
    if (inp.Contains("Object."))
        return "object";
    if (inp.Contains("function"))
        return "Function";
    if (inp.Contains("integer"))
        return "number";
    if (inp.Contains("Array."))
        inp = inp.Replace("Array.", "Array");
    if (inp.Contains('~'))
        inp = inp.Replace("~", ".");
    if (inp.Contains("|"))
        inp = inp.Split('|').First();

    if (inp != "string" && inp != "number" && inp != "boolean" && inp != "object" && !inp.Contains("Array"))
        inp = "typeof " + inp;

    if (inp.Contains("array"))
    {
        if (inp.Contains("string") || inp.Contains("number") || inp.Contains("boolean") || inp.Contains("object"))
            inp = inp.Replace("array", "Array");
        else
            inp = inp.Replace("array<", "Array< typeof");
    }

    return inp;
}

TypeScriptDeclaration get(HifiJsDoc data)
{
    var name = (data.Memberof ?? data.Name).Replace("-", "");
    var dl = declarations.FirstOrDefault(x => x.Name == name);

    if (dl != null) return dl;

    var category_name = data.Kind switch
    {
        Kind.Class => "class",
        Kind.Function => "namespace",
        Kind.Namespace => "namespace",
        Kind.Signal => "namespace",
        Kind.Typedef => "class",
        _ => "",
    };

    dl = new TypeScriptDeclaration()
    {
        Name = name,
        Description = data.Description.Replace("\n", "\n // "),
        Type = category_name
    };
    declarations.Add(dl);

    return dl;
}

string? CleanDescription(string? des) => des?.Replace("\n", "\n\t// ");