namespace KellySelden.Libraries.Expressions
{
	public class ExpressionBranch : IExpressionNode
	{
		public IExpressionNode Left { get; set; }
		public IExpressionNode Right { get; set; }
		public string Operator { get; set; }
		public string Expression { get; set; }
	}
}