namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Simple Trick to get Extension methods in .Net 2 
    /// See : http://csharpindepth.com/Articles/Chapter1/Versions.aspx
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class ExtensionAttribute : Attribute
    {
    }
}