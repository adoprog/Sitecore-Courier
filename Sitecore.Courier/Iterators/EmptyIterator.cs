using Sitecore.Update.Interfaces;

namespace Sitecore.Courier.Iterators
{
    class EmptyIterator : IDataIterator
    {
        public IDataItem Next()
        {
            return null;
        }
    }
}
