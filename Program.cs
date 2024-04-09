using System.Collections.Generic;
using System.IO;
using System.Linq;
using Domain = System.Collections.Generic.HashSet<System.Collections.Generic.KeyValuePair<int, int>>;
using Value = int;
using Variable = int;
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

public sealed class SRT
{
    private readonly List<Tuple> table = new List<Tuple>();

    public void Add(Tuple t)
    {
        table.Add(t);
    }

    public (int, Domain) Run(int currentLimit, Domain domain)
    {
        var gacValues = new Domain();

        // remove invalid tuples from table
        for (int i = currentLimit - 1; i >= 0; i--)
        {
            var t = table[i];
            if (isValidTuple(t, domain))
            {
                foreach (var v in t.vs)
                {
                    gacValues.Add(v);
                }
            }
            else
            {
                var last = currentLimit - 1;
                table[i] = table[last];
                table[last] = t;
                currentLimit--;
            }
        }

        return (currentLimit, gacValues);
    }

    private bool isValidTuple(Tuple t, Domain domain)
    {
        foreach (var v in t.vs)
        {
            if (!domain.Contains(v))
            {
                return false;
            }
        }
        return true;
    }

    public void WriteTo(TextWriter writer)
    {
        for (int i = 0; i < table.Count; i++)
        {
            writer.Write(i);
            writer.Write(": ");
            table[i].WriteTo(writer);
            writer.WriteLine();
        }
    }
}


internal static class Program
{
    private static void Main(string[] args)
    {
        Variable x = 0;
        Variable y = 1;
        Variable z = 2;
        Value a = 0;
        Value b = 1;
        Value c = 2;

        var srt = new SRT();
        srt.Add(Create(a, a, a));
        srt.Add(Create(a, a, b));
        srt.Add(Create(a, b, b));
        srt.Add(Create(b, a, a));
        srt.Add(Create(b, a, b));
        srt.Add(Create(b, b, c));
        srt.Add(Create(b, c, a));
        srt.Add(Create(c, a, a));
        srt.Add(Create(c, b, a));
        srt.Add(Create(c, c, a));

        var limit = 10;

        var domain = new Domain();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                domain.Add(KeyValuePair.Create(i, j));
            }
        }

        domain.Remove(KeyValuePair.Create(y, b));

        var writer = System.Console.Out;
        writer.WriteLine("Table");
        srt.WriteTo(writer);
        writer.WriteLine();

        writer.Write("Limit: ");
        writer.WriteLine(limit);
        writer.WriteLine();

        writer.WriteLine("Domain");
        domain.WriteTo(writer);
        writer.WriteLine();

        var (reducedLimit, reducedDomain) = srt.Run(limit, domain);

        writer.WriteLine("Reduced Table");
        srt.WriteTo(writer);
        writer.WriteLine();

        writer.Write("Reduced Limit: ");
        writer.WriteLine(reducedLimit);
        writer.WriteLine();

        writer.WriteLine("Reduced Domain");
        reducedDomain.WriteTo(writer);
        writer.WriteLine();
    }

    private static Tuple Create(int x, int y, int z)
    {
        return new Tuple
        {
            vs = new[] {
                    KeyValuePair.Create(0, x),
                    KeyValuePair.Create(1, y),
                    KeyValuePair.Create(2, z),
                },
        };
    }
}
