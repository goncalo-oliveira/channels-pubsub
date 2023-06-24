using System.Collections;

internal sealed class ConcurrentList<T> : IEnumerable<T>
{
    private readonly List<T> list = new List<T>();
    private readonly object sync = new object();

    //

    public void TryAdd( T item )
    {
        lock ( sync )
        {
            if ( !list.Contains( item ) )
            {
                list.Add( item );
            }
        }
    }

    public void TryRemove( T item )
    {
        lock ( sync )
        {
            list.Remove( item );
        }
    }

    //

    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
