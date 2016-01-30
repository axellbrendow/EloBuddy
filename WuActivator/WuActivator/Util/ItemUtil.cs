using EloBuddy;
using EloBuddy.SDK;

namespace WuActivator.Util
{
    class ItemUtil
    {
        public Item GetItem(ItemId id)
        {
            return new Item(id);
        }

        public Item GetItem(int id)
        {
            return new Item(id);
        }

        public Item GetItem(ItemId id, int range)
        {
            return new Item(id, range);
        }

        public Item GetItem(int id, int range)
        {
            return new Item(id, range);
        }
    }
}
