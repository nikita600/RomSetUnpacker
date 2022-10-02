using System;
using System.Collections.Generic;
using System.IO;

namespace RomSetUnpacker
{
	internal static class Program
	{
		private static readonly Dictionary<string, Action<string[]>> Actions = new()
		{
			{ "unpack", UnpackRomSet },
			{ "extract_groups", ExtractGroups }
		};
		
		private static void Main(string[] args)
		{
			//ArchiveOrgLinksParser.ParseLinks(args[0]);
			//return;
			
			Console.WriteLine("No-Intro Rom Unpacker by nikita600 19/02/2022");
			Console.WriteLine("Using: RomSetUnpacker.exe <options>");
			Console.WriteLine("Options: \n" +
			                  "\t unpack <input_archive> <output_folder> -- unpacks No-Intro ROM set." +
			                  "\t extract_groups <input_archive> <output_groups_list> -- reads all group tags in ROM set.");

			if (TryGetArg(args, 0, out var option) 
			    && Actions.TryGetValue(option, out var action))
			{
				action?.Invoke(args);
			}
			
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}

		private static bool TryGetArg(IReadOnlyList<string> args, int index, out string argument)
		{
			if (index < args.Count)
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

			var (cursorLeft, cursorTop) = Console.GetCursorPosition();
			
			var progress = Math.Floor(current * 100f / size);
			Console.WriteLine("Progress: " + progress + "%");

			if (returnCursor)
			{
				Console.SetCursorPosition(cursorLeft, cursorTop);
			}
		}
		
		private static void UnpackRomSet(string[] args)
		{
			if (!TryGetArg(args, 1, out var inputPath)
			    || !TryGetArg(args, 2, out var outputPath))
			{
				return;
			}
			
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

		private static void ExtractGroups(string[] args)
		{
			
		}
	}
}
