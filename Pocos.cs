class TypeScriptDeclaration
{
    public string Name = "";
    public string Type = "";
    public string Description = "";
    public List<TypeScriptVariable> Variables = [];
    public List<TypeScriptFunction> Functions = [];
    public List<TypeScriptDeclaration> Subclasses = [];
    public string? Extra;
}

class TypeScriptVariable
{
    public string Name = "";
    public string? Description;
    public string Type = "";

    public bool Equals(TypeScriptVariable other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name.Equals(other.Name);
    }
}

class TypeScriptFunction
{
    public string Name = "";
    public string? Description;
    public string Type = "";

    public bool Equals(TypeScriptFunction other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name.Equals(other.Name);
    }
}