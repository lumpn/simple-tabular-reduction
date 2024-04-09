using System.IO;
using VValue = System.Collections.Generic.KeyValuePair<int, int>;

public struct Tuple
{
    public VValue[] vs;

    public void WriteTo(TextWriter writer)
    {
        writer.Write("(");
        for (int i = 0; i < vs.Length; i++)
        {
            var v = vs[i];
            v.WriteTo(writer);
            if (i < vs.Length - 1)
            {
                writer.Write(", ");
            }
        }
        writer.Write(")");
    }
}
