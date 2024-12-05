namespace Overte;

using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class HifiJsDoc
{
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }

    [JsonProperty("kind")]
    public Kind Kind { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("hifiAssignmentClient", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HifiAssignmentClient { get; set; }

    [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
    public Param[]? Properties { get; set; }

    [JsonProperty("longname")]
    public string Longname { get; set; }

    [JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
    public Scope? Scope { get; set; }

    [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
    public Param[] Params { get; set; }

    [JsonProperty("examples", NullValueHandling = NullValueHandling.Ignore)]
    public string[] Examples { get; set; }

    [JsonProperty("memberof", NullValueHandling = NullValueHandling.Ignore)]
    public string? Memberof { get; set; }

    [JsonProperty("returns", NullValueHandling = NullValueHandling.Ignore)]
    public Return[]? Returns { get; set; }

    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public Tag[]? Tags { get; set; }

    [JsonProperty("deprecated", NullValueHandling = NullValueHandling.Ignore)]
    public string? Deprecated { get; set; }

    [JsonProperty("hifiInterface", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HifiInterface { get; set; }

    [JsonProperty("hifiClientEntity", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HifiClientEntity { get; set; }

    [JsonProperty("hifiAvatar", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HifiAvatar { get; set; }

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public TypeClass? Type { get; set; }

    [JsonProperty("see", NullValueHandling = NullValueHandling.Ignore)]
    public string[] See { get; set; }

    [JsonProperty("hifiServerEntity", NullValueHandling = NullValueHandling.Ignore)]
    public bool? HifiServerEntity { get; set; }

    [JsonProperty("variation", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(ParseStringConverter))]
    public long? Variation { get; set; }

    [JsonProperty("hideconstructor", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Hideconstructor { get; set; }

    [JsonProperty("exceptions", NullValueHandling = NullValueHandling.Ignore)]
    public ExceptionElement[] Exceptions { get; set; }

    [JsonProperty("readonly", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Readonly { get; set; }

    [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
    public string[] Files { get; set; }
}

public partial class ExceptionElement
{
    [JsonProperty("description")]
    public string Description { get; set; }
}

public partial class Param
{
    [JsonProperty("type")]
    public TypeClass? Type { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; set; }

    [JsonProperty("optional", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Optional { get; set; }

    [JsonProperty("defaultvalue")]
    public Defaultvalue? Defaultvalue { get; set; }

    [JsonProperty("variable", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Variable { get; set; }
}

public partial class TypeClass
{
    [JsonProperty("names")]
    public string[] Names { get; set; }
}

public partial class Return
{
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public TypeClass? Type { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }
}

public partial class Tag
{
    [JsonProperty("originalTitle")]
    public Title OriginalTitle { get; set; }

    [JsonProperty("title")]
    public Title Title { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }
}

public enum Kind { Class, Function, Namespace, Package, Signal, Typedef };

public enum Scope { Global, Inner, Instance, Static };

public enum Title { Comment, Exammple, Paramm, Parm };

public partial struct Defaultvalue
{
    public bool? Bool;
    public double? Double;
    public string String;

    public static implicit operator Defaultvalue(bool Bool) => new Defaultvalue { Bool = Bool };
    public static implicit operator Defaultvalue(double Double) => new Defaultvalue { Double = Double };
    public static implicit operator Defaultvalue(string String) => new Defaultvalue { String = String };
    public bool IsNull => Bool == null && Double == null && String == null;
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            KindConverter.Singleton,
            DefaultvalueConverter.Singleton,
            ScopeConverter.Singleton,
            TitleConverter.Singleton,
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
    };
}

internal class KindConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Kind) || t == typeof(Kind?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        switch (value)
        {
            case "class":
                return Kind.Class;
            case "function":
                return Kind.Function;
            case "namespace":
                return Kind.Namespace;
            case "package":
                return Kind.Package;
            case "signal":
                return Kind.Signal;
            case "typedef":
                return Kind.Typedef;
        }
        throw new Exception("Cannot unmarshal type Kind");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (Kind)untypedValue;
        switch (value)
        {
            case Kind.Class:
                serializer.Serialize(writer, "class");
                return;
            case Kind.Function:
                serializer.Serialize(writer, "function");
                return;
            case Kind.Namespace:
                serializer.Serialize(writer, "namespace");
                return;
            case Kind.Package:
                serializer.Serialize(writer, "package");
                return;
            case Kind.Signal:
                serializer.Serialize(writer, "signal");
                return;
            case Kind.Typedef:
                serializer.Serialize(writer, "typedef");
                return;
        }
        throw new Exception("Cannot marshal type Kind");
    }

    public static readonly KindConverter Singleton = new KindConverter();
}

internal class DefaultvalueConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Defaultvalue) || t == typeof(Defaultvalue?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Null:
                return new Defaultvalue { };
            case JsonToken.Integer:
            case JsonToken.Float:
                var doubleValue = serializer.Deserialize<double>(reader);
                return new Defaultvalue { Double = doubleValue };
            case JsonToken.Boolean:
                var boolValue = serializer.Deserialize<bool>(reader);
                return new Defaultvalue { Bool = boolValue };
            case JsonToken.String:
            case JsonToken.Date:
                var stringValue = serializer.Deserialize<string>(reader);
                return new Defaultvalue { String = stringValue };
        }
        throw new Exception("Cannot unmarshal type Defaultvalue");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        var value = (Defaultvalue)untypedValue;
        if (value.IsNull)
        {
            serializer.Serialize(writer, null);
            return;
        }
        if (value.Double != null)
        {
            serializer.Serialize(writer, value.Double.Value);
            return;
        }
        if (value.Bool != null)
        {
            serializer.Serialize(writer, value.Bool.Value);
            return;
        }
        if (value.String != null)
        {
            serializer.Serialize(writer, value.String);
            return;
        }
        throw new Exception("Cannot marshal type Defaultvalue");
    }

    public static readonly DefaultvalueConverter Singleton = new DefaultvalueConverter();
}

internal class ScopeConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Scope) || t == typeof(Scope?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        switch (value)
        {
            case "global":
                return Scope.Global;
            case "inner":
                return Scope.Inner;
            case "instance":
                return Scope.Instance;
            case "static":
                return Scope.Static;
        }
        throw new Exception("Cannot unmarshal type Scope");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (Scope)untypedValue;
        switch (value)
        {
            case Scope.Global:
                serializer.Serialize(writer, "global");
                return;
            case Scope.Inner:
                serializer.Serialize(writer, "inner");
                return;
            case Scope.Instance:
                serializer.Serialize(writer, "instance");
                return;
            case Scope.Static:
                serializer.Serialize(writer, "static");
                return;
        }
        throw new Exception("Cannot marshal type Scope");
    }

    public static readonly ScopeConverter Singleton = new ScopeConverter();
}

internal class TitleConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(Title) || t == typeof(Title?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        switch (value)
        {
            case "comment":
                return Title.Comment;
            case "exammple":
                return Title.Exammple;
            case "paramm":
                return Title.Paramm;
            case "parm":
                return Title.Parm;
        }
        throw new Exception("Cannot unmarshal type Title");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (Title)untypedValue;
        switch (value)
        {
            case Title.Comment:
                serializer.Serialize(writer, "comment");
                return;
            case Title.Exammple:
                serializer.Serialize(writer, "exammple");
                return;
            case Title.Paramm:
                serializer.Serialize(writer, "paramm");
                return;
            case Title.Parm:
                serializer.Serialize(writer, "parm");
                return;
        }
        throw new Exception("Cannot marshal type Title");
    }

    public static readonly TitleConverter Singleton = new TitleConverter();
}

internal class ParseStringConverter : JsonConverter
{
    public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var value = serializer.Deserialize<string>(reader);
        long l;
        if (Int64.TryParse(value, out l))
        {
            return l;
        }
        throw new Exception("Cannot unmarshal type long");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
        if (untypedValue == null)
        {
            serializer.Serialize(writer, null);
            return;
        }
        var value = (long)untypedValue;
        serializer.Serialize(writer, value.ToString());
        return;
    }

    public static readonly ParseStringConverter Singleton = new ParseStringConverter();
}
