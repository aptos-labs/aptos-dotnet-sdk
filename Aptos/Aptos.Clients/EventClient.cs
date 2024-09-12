namespace Aptos;

using Aptos.Indexer.GraphQL;

public class EventClient(AptosClient client)
{

    private readonly AptosClient _client = client;

    /// <summary>
    /// Gets the events that match the given condition.
    /// </summary>
    /// <param name="where">The condition to filter the events.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>A list of events.</returns>
    public async Task<List<EventData>> GetEvents(events_bool_exp where, int offset = 0, int limit = 50, events_order_by? orderBy = null)
    {
        return (await _client.Indexer.Query(async client => await client.GetEvents.ExecuteAsync(where, offset, limit, orderBy != null ? [orderBy] : []))).Data.Events.Select(e =>
        {
            try
            {
                return new EventData(e);
            }
            catch
            {
                return null;
            }
        }).Where(e => e != null).Cast<EventData>().ToList();
    }

}