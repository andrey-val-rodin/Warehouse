namespace WarehouseFiller
{
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public int Amount { get; set; }
        public int? Price { get; set; }
        public bool IsUnit { get; set; }
    }
}
