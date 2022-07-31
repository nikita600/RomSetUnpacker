using System;
using System.IO;

namespace RomSetUnpacker
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("No-Intro Rom Unpacker by nikita600 19/02/2022");
			Console.WriteLine("Using: RomSetUnpacker.exe <input_archive> <output_folder>");

			if (TryGetArg(args, 0, out var inputPath) 
				&& TryGetArg(args, 1, out var outputPath))
			{
				if (Directory.Exists(outputPath))
				{
					var files = Directory.GetFiles(outputPath);
					if (files.Length > 0)
					{
						throw new Exception("Output directory exists and not empty. Path: " + outputPath);
					}
				}
				else
				{
					Directory.CreateDirectory(outputPath);
				}

				try
				{
					if (File.Exists(inputPath))
					{
						if (Asset.IsZipArchive(inputPath))
						{
							var romSetAsset = new RomSetAsset();
							romSetAsset.Unpack(inputPath, outputPath, 
								(progress, size) => ShowProgressString(progress, size, progress < size));
						}
						else
						{
							throw new ArgumentException("Input file is not a ZIP file: " + inputPath);
						}
					}
					else
					{
						throw new Exception("Input file not exist.");
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception occured: " + e.Message);
				}
				

				Console.WriteLine("Finished!");
			}

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}

		private static bool TryGetArg(string[] args, int index, out string argument)
		{
			if (index < args.Length)
			{
				argument = args[index];
				return true;
			}

			argument = string.Empty;
			return false;
		}
		
		private static void ShowProgressString(float current, float size, bool returnCursor)
		{
			if (Console.IsOutputRedirected)
			{
				return;
			}

			var progress = Math.Floor(current * 100f / size);
			var cursorPosition = Console.GetCursorPosition();
			Console.WriteLine("Progress: " + progress + "%");

			if (returnCursor)
			{
				Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
			}
		}
	}
}
