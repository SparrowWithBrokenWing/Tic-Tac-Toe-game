using MessageConstruction;
using MessageDispatcher;

namespace Match
{
    public class CompositeAsyncPerformer : ICompositeAsyncPerformer
    {
        private IList<IAsyncPerformer> _Performers;

        public CompositeAsyncPerformer(IEnumerable<IAsyncPerformer> performers)
        {
            _Performers = performers.ToList();
        }

        public CompositeAsyncPerformer()
        {
            _Performers = new List<IAsyncPerformer>();
        }

        public void Add(IAsyncPerformer performer)
        {
            _Performers.Add(performer);
        }

        public void Remove(IAsyncPerformer performer)
        {
            _Performers.Remove(performer);
        }

        public async Task ProcessAsync(IMessage message)
        {
            var tasks = new Task[_Performers.Count];
            for (int i = 0; i < _Performers.Count; i++)
            {
                // message should be cloned here and performer should receive clone only.
                tasks[i] = _Performers[i].ProcessAsync(message);
            }
            await Task.WhenAll(tasks);
        }
    }
}
