using LuaApiBuilder;

string inputPath = args.Length > 0 ? args[0] : "API.xml";
string outputPath = args.Length > 1 ? args[1] : ".api";

if (!File.Exists(inputPath))
{
	Console.WriteLine($"Error: API.xml file not found at '{inputPath}'");
	Console.WriteLine("Usage: LuaApiBuilder.exe [input_xml_path] [output_directory]");
	return;
}

try
{
	var converter = new ApiConverter();
	converter.Convert(inputPath, outputPath);

	Console.WriteLine($"Successfully generated Lua definition files in '{outputPath}'");
}
catch (Exception ex)
{
	Console.WriteLine($"Error: {ex.Message}");
	Console.WriteLine(ex.StackTrace);
}
