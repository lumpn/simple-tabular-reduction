using System.IO;
using System.Linq;
using Domain = System.Collections.Generic.HashSet<System.Collections.Generic.KeyValuePair<int, int>>;
using VValue = System.Collections.Generic.KeyValuePair<int, int>;

public static class WriteExtentions
{
    public static void WriteTo(this VValue v, TextWriter writer)
    {
        writer.Write("(");
        writer.Write(v.Key);
        writer.Write(", ");
        writer.Write(v.Value);
        writer.Write(")");
    }

    public static void WriteTo(this Domain domain, TextWriter writer)
    {
        foreach (var v in domain.OrderBy(p => p.Key).ThenBy(p => p.Value))
        {
            v.WriteTo(writer);
            writer.WriteLine();
        }
    }

}
