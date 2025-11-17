namespace HubDocs;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class HubDocsAttribute : Attribute
{
    public HubDocsAttribute()
    {
    }
}