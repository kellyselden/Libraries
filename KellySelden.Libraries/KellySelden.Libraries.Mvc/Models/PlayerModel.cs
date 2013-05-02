namespace KellySelden.Libraries.Mvc.Models
{
	public class PlayerModel
	{
		public PlayerType Type { get; set; }
		public string FilePath { get; set; }
		public string SplashPath { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
	}
	public enum PlayerType { Audio, Video, YouTube }
}