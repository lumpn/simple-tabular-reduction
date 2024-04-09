using System.Collections.Generic;
using System.IO;
using Domain = System.Collections.Generic.HashSet<System.Collections.Generic.KeyValuePair<int, int>>;

public sealed class SRT
{
    private readonly HashSet<int> scope = new HashSet<int>();
    private readonly List<Tuple> table = new List<Tuple>();
    public int currentLimit = 0;

    public void Add(Tuple t)
    {
        foreach (var v in t.vs)
        {
            scope.Add(v.Key);
        }
        table.Add(t);
        currentLimit++;
    }

    public bool Run(Domain domain, Domain reducedDomain)
    {
        reducedDomain.Clear();
        var gacValues = reducedDomain;

        // remove invalid tuples from table
        for (int i = currentLimit - 1; i >= 0; i--)
        {
            var t = table[i];
            if (isValidTuple(t, domain))
            {
                gacValues.UnionWith(t.vs);
            }
            else
            {
                var last = currentLimit - 1;
                table[i] = table[last];
                table[last] = t;
                currentLimit--;
            }
        }

        // keep out-of-scope variables in domain
        var isValid = true;
        foreach (var v in domain)
        {
            var variable = v.Key;
            if (scope.Contains(variable))
            {
                // make sure the reduced domain contains a non-empty set
                var found = false;
                foreach (var w in reducedDomain)
                {
                    if (w.Key == variable)
                    {
                        found = true;
                        break;
                    }
                }
                isValid &= found;
            }
            else
            {
                reducedDomain.Add(v);
            }
        }

        return isValid;
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
        for (int i = 0; i < currentLimit; i++)
        {
            writer.Write(i);
            writer.Write(": ");
            table[i].WriteTo(writer);
            writer.WriteLine();
        }
    }
}
