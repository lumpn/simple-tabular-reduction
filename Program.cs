using System.Collections.Generic;
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


internal static class Program
{
    private static void Main(string[] args)
    {
        MagicSquare3x3();
    }

    private static void MagicSquare3x3()
    {
        var writer = System.Console.Out;

        // values: 1, .., 9
        // positions: 3x3 matrix indexed 0, .., 8
        // sum of each row, colum, and diagonal: 15
        // no number used twice
        // top-left cell value: 2 (initial state)

        // generate a list of tuples that add up to 15
        var candidates = new List<Tuple>();
        for (int i = 1; i < 10; i++)
        {
            for (int j = i + 1; j < 10; j++)
            {
                for (int k = j + 1; k < 10; k++)
                {
                    if (i + j + k == 15)
                    {
                        candidates.Add(Create(i, j, k));
                        candidates.Add(Create(i, k, j));
                        candidates.Add(Create(j, i, k));
                        candidates.Add(Create(j, k, i));
                        candidates.Add(Create(k, i, j));
                        candidates.Add(Create(k, j, i));
                    }
                }
            }
        }
        writer.WriteLine("{0} candidates", candidates.Count);

        // generate rows, columns, and diagonals
        var m00 = 0;
        var m01 = 1;
        var m02 = 2;
        var m10 = 3;
        var m11 = 4;
        var m12 = 5;
        var m20 = 6;
        var m21 = 7;
        var m22 = 8;

        var lines = new List<Tuple>();

        // generate rows
        lines.Add(Create(m00, m01, m02));
        lines.Add(Create(m10, m11, m12));
        lines.Add(Create(m10, m11, m12));

        // generate colums
        lines.Add(Create(m00, m10, m20));
        lines.Add(Create(m01, m11, m21));
        lines.Add(Create(m02, m12, m22));

        // generate diagonals
        lines.Add(Create(m00, m11, m22));
        lines.Add(Create(m02, m11, m20));

        writer.WriteLine("{0} lines", lines.Count);

        // generate constraints
        var constraints = new List<SRT>();
        var vValues = new List<VValue>();
        foreach (var line in lines)
        {
            var lvs = line.vs;
            var srt = new SRT();
            foreach (var candidate in candidates)
            {
                var cvs = candidate.vs;
                for (int i = 0; i < lvs.Length; i++)
                {
                    var v = KeyValuePair.Create(lvs[i].Value, cvs[i].Value);
                    vValues.Add(v);
                }

                var tuple = new Tuple
                {
                    vs = vValues.ToArray(),
                };
                vValues.Clear();
                srt.Add(tuple);
            }
            constraints.Add(srt);
        }

        writer.WriteLine("{0} constraints", constraints.Count);

        var domain = new Domain();
        domain.Add(KeyValuePair.Create(m00, 2)); // initial state 
        for (int i = m01; i < 9; i++)
        {
            for (int j = 1; j < 10; j++)
            {
                domain.Add(KeyValuePair.Create(i, j));
            }
        }

        writer.WriteLine("domain before: {0}", domain.Count);

        var domain2 = new Domain();
        var variables = domain.Select(p => p.Key).Distinct().ToArray();
        var result = Solve(constraints, domain, domain2, variables, writer);
        switch (result)
        {
            case SolutionType.None: writer.WriteLine("inconclusive"); break;
            case SolutionType.Solved: writer.WriteLine("solved"); break;
            case SolutionType.Failed: writer.WriteLine("failed"); break;
        }

        writer.WriteLine("domain after: {0}", domain.Count);
        domain.WriteTo(writer);
    }

    private enum SolutionType
    {
        None,
        Solved,
        Failed,
    }

    private static SolutionType Solve(List<SRT> constraints, Domain domain, Domain domain2, int[] variables, TextWriter writer)
    {
        int domainSizeBeforeRun, domainSizeAfterRun;
        do
        {
            domainSizeBeforeRun = domain.Count;
            var domainSizeBeforeStep = domainSizeBeforeRun;
            foreach (var constraint in constraints)
            {
                //writer.WriteLine("constraint before step");
                //constraint.WriteTo(writer);
                //writer.WriteLine();

                var isValid = constraint.Run(domain, domain2);
                var tmp = domain2;
                domain2 = domain;
                domain = tmp;

                var domainSizeAfterStep = domain.Count;
                if (domainSizeAfterStep < domainSizeBeforeStep)
                {
                    //writer.WriteLine("constraint after step");
                    //constraint.WriteTo(writer);
                    //writer.WriteLine("domain after step: {0}", domain.Count);
                    //domain.WriteTo(writer);
                    //writer.WriteLine();
                    domainSizeBeforeStep = domainSizeAfterStep;
                }

                if (!isValid)
                {
                    writer.WriteLine("failure");
                    return SolutionType.Failed;
                }
            }
            writer.WriteLine("domain after run: {0}", domain.Count);
            domainSizeAfterRun = domain.Count;
        }
        while (domainSizeAfterRun < domainSizeBeforeRun);

        // no more forward progress. solved?
        var solved = true;
        foreach (var variable in variables)
        {
            var count = domain.Count(p => p.Key == variable);
            if (count == 0)
            {
                return SolutionType.Failed;
            }
            if (count > 1)
            {
                solved = false;
                break;
            }
        }

        if (solved)
        {
            return SolutionType.Solved;
        }

        writer.WriteLine("domain before guess {0}", domain.Count);
        domain.WriteTo(writer);
        writer.WriteLine();

        // guess a v-value!
        var originalLimits = constraints.Select(p => p.currentLimit).ToArray();
        var originalDomain = new Domain(domain);
        foreach (var v in originalDomain.OrderBy(p => p.Key)) // TODO Jonas: guess most constrained variable first
        {
            var count = originalDomain.Count(p => p.Key == v.Key);
            if (count < 2) continue;

            // lock in choice
            writer.WriteLine("guessing {0} := {1}", v.Key, v.Value);
            domain.Clear();
            domain.Add(v);
            foreach (var w in originalDomain)
            {
                if (w.Key != v.Key)
                {
                    domain.Add(w);
                }
            }

            // recurse
            var solutionType = Solve(constraints, domain, domain2, variables, writer);
            switch (solutionType)
            {
                case SolutionType.None:
                case SolutionType.Solved:
                    return solutionType;
            }

            // failed. backtrack! restore original limits.
            writer.WriteLine("backtracking");
            for (int i = 0; i < originalLimits.Length; i++)
            {
                constraints[i].currentLimit = originalLimits[i];
            }
        }

        // we have guessed all possible values and nothing worked
        return SolutionType.Failed;
    }

    private static void PaperExample()
    {
        var x = 0;
        var y = 1;
        var z = 2;
        var a = 0;
        var b = 1;
        var c = 2;

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

        writer.WriteLine("Domain");
        domain.WriteTo(writer);
        writer.WriteLine();

        var reducedDomain = new Domain();
        srt.Run(domain, reducedDomain);

        writer.WriteLine("Reduced Table");
        srt.WriteTo(writer);
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
