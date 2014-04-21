using System.Globalization;
using KellySelden.Libraries.Excel.Services.NPOI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests.Excel
{
	[TestClass]
	public class NPOICultureTests
	{
		[TestMethod]
		[DeploymentItem(Strings.DeploymentItem)]
		public void ExcelService_CultureCompare()
		{
			Assert.IsFalse(ExcelCompare.Compare(Strings.FileName, new ExcelService(new CultureInfo("en-US")), new ExcelService(new CultureInfo("de-DE"))));
		}
	}
}