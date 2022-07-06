namespace DynamicGridEdit.Models
{
    public class HomeViewModel
    {
        public class QueryOut
        {
            public List<SaleModel> grids { get; set; }
        }

        public class SaleModel
        {
            public string PK { get; set; }
            public string Name { get; set; }
            public string Item { get; set; }
            public string Qty { get; set; }
            public string Amount { get; set; }
            public string Date { get; set; }
        }

        public class UpdateToDbIn
        {
            public List<SaleModel> grids { get; set; }
        }

        public class UpdateToDbOut
        {
            public string msg { get; set; }
        }

    }
}
