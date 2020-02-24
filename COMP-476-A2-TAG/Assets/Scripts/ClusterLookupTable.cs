using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClusterLookupTable<T> where T : IComparable<T>
{
    private T[,] table;

    public ClusterLookupTable(int cluster_count)
    {
        table = new T[cluster_count, cluster_count];
    }

    public T this[int row, int column]
    {
        get { return table[row, column]; }
        set { table[row, column] = value; }
    }
}
