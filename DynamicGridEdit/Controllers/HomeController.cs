using Dapper;
using DynamicGridEdit.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using static DynamicGridEdit.Models.HomeViewModel;

namespace DynamicGridEdit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// 載入資料
        /// </summary>
        /// <param name="inModel"></param>
        /// <returns></returns>
        public IActionResult Query()
        {
            QueryOut outModel = new QueryOut();
            outModel.grids = new List<SaleModel>();

            // 資料庫連線字串
            IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            string connStr = Config.GetConnectionString("SqlServer");

            //查詢 SQL
            SqlConnection conn = new SqlConnection(connStr);
            string sql = @"SELECT Pk,Name, Item, Qty, Amount, CONVERT(varchar(12),Date,111) AS Date 
                            FROM Sale ORDER BY PK";
            outModel.grids = (List<SaleModel>)conn.Query<SaleModel>(sql);// 使用 Dapper 查詢

            return Json(outModel);
        }

        /// <summary>
        /// 更新至資料庫
        /// </summary>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult UpdateToDb(UpdateToDbIn inModel)
        {
            UpdateToDbOut outModel = new UpdateToDbOut();

            // 資料庫連線字串
            IConfiguration Config = new ConfigurationBuilder().AddJsonFile("appSettings.json").Build();
            string connStr = Config.GetConnectionString("SqlServer");

            // 先取得原有資料庫內資料
            SqlConnection conn = new SqlConnection(connStr);
            string sql = @"SELECT Pk,Name, Item, Qty, Amount, CONVERT(varchar(12),Date,111) AS Date 
					FROM Sale ORDER BY PK";
            var orgSale = conn.Query<SaleModel>(sql);// 使用 Dapper 查詢

            //如果 Pk 欄位為空值則新增
            foreach (SaleModel newSale in inModel.grids)
            {
                if (string.IsNullOrEmpty(newSale.PK))
                {
                    sql = @"INSERT INTO Sale([Name], Item, Qty, Amount, [Date])
					VALUES      (@Name, @Item, @Qty, @Amount, @Date)";
                    var param = new
                    {
                        Name = newSale.Name,
                        Item = newSale.Item,
                        Qty = newSale.Qty,
                        Amount = newSale.Amount,
                        Date = newSale.Date,
                    };
                    conn.Execute(sql, param);// 使用 Dapper
                }
            }

            // 如果 Pk 值存在於原始資料則更新資料
            foreach (SaleModel newSale in inModel.grids)
            {
                if (orgSale.Any(w => w.PK == newSale.PK))
                {
                    sql = @"UPDATE Sale
					SET    [Name] = @Name, Item = @Item, Qty = @Qty, Amount = @Amount, [Date] = @Date
					WHERE  Pk = @Pk";
                    var param = new
                    {
                        Name = newSale.Name,
                        Item = newSale.Item,
                        Qty = newSale.Qty,
                        Amount = newSale.Amount,
                        Date = newSale.Date,
                        Pk = newSale.PK
                    };
                    conn.Execute(sql, param);// 使用 Dapper
                }
            }

            //如果原始資料不存在輸入的 Grid 則刪除
            foreach (SaleModel sale in orgSale)
            {
                if (!inModel.grids.Any(w => w.PK == sale.PK))
                {
                    sql = @"DELETE FROM Sale WHERE Pk = @Pk";
                    var param = new
                    {
                        Pk = sale.PK
                    };
                    conn.Execute(sql, param);// 使用 Dapper
                }
            }

            outModel.msg = "全部更新完成";

            return Json(outModel);
        }
    }
}