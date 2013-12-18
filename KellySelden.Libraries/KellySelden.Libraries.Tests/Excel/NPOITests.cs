using System.Linq;
using KellySelden.Libraries.Excel.Services.NPOI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests.Excel
{
	[TestClass]
	public class NPOITests : ExcelTests
	{
		const string FileName = "ie_data.xls",
		             DeploymentItem = "Excel/" + FileName,
		             WorksheetName = "Data";

		public NPOITests()
			: base(FileName, new ExcelService()) { }

		[TestInitialize]
		public override void TestInitialize()
		{
			base.TestInitialize();
		}

		[TestMethod]
		[DeploymentItem(DeploymentItem)]
		public override void ExcelService_OpenWorkbook_FileNotFound_ReturnsNull()
		{
			base.ExcelService_OpenWorkbook_FileNotFound_ReturnsNull();
		}

		[TestMethod]
		[DeploymentItem(DeploymentItem)]
		public void Workbook_Worksheets()
		{
			var worksheets = Workbook.Worksheets.ToArray();

			Assert.AreEqual(3, worksheets.Length);
			Assert.AreEqual("Index Plot", worksheets[0].Name);
			Assert.AreEqual("PE (CAPE) Plot", worksheets[1].Name);
			Assert.AreEqual(WorksheetName, worksheets[2].Name);
		}

		[TestMethod]
		[DeploymentItem(DeploymentItem)]
		public void Worksheet_Rows()
		{
			Assert.AreEqual(2423, Workbook.Worksheets.Single(worksheet => worksheet.Name == WorksheetName).Rows.Count());
		}

		[TestMethod]
		[DeploymentItem(DeploymentItem)]
		public override void ExcelService_CheckGrid()
		{
			base.ExcelService_CheckGrid();
		}
	}
}