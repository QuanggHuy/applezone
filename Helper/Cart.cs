namespace AppleZone.Helper
{
    public class Cart
    {
        private SortedList<int, Item> list;
        public Cart()
        {
            list = new SortedList<int, Item>();
        }

        public SortedList<int, Item> List
        {
            get
            {
                return list;
            }
        }

        public void Add(Item item)
        {
            //Nếu item đã có trong List thì cập nhật Quantity, ngược lại thì thêm item vào List
            if (List.ContainsKey(item.Id))
            {
                Item currentItem = List[item.Id];
                currentItem.Quantity += item.Quantity;
            }
            else
            {
                List.Add(item.Id, item);
            }
        }

        public void Remove(int id)
        {
            List.Remove(id);
        }
        public void Empty()
        {
            List.Clear();
        }

        public void Update(int id, int quantity)
        {
            Item item = List[id];
            if (item != null)
            {
                if (quantity <= 0)
                    Remove(id);
                else
                    item.Quantity = quantity;
            }
        }

        public double TotalAmount
        {
            get
            {
                return List.Values.Sum(item => item.Amount);
            }
        }
    }
}
