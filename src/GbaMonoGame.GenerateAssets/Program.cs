// Hacky code, but it's temporary until MonoGame gets updated with the new content pipeline...

using System.Text;

string dir = args[0];
string csBaseDir = args[1];

StringBuilder mgcb = new();
mgcb.Append("""

            #----------------------------- Global Properties ----------------------------#

            /outputDir:bin/$(Platform)
            /intermediateDir:obj/$(Platform)
            /platform:Windows
            /config:
            /profile:Reach
            /compress:False

            #-------------------------------- References --------------------------------#


            #---------------------------------- Content ---------------------------------#


            """);

Dictionary<string, List<(string Name, string Path)>> assets = new();
foreach (string filePath in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
{
    string fileName = Path.GetFileName(filePath);
    string fileDir = Path.GetDirectoryName(filePath) ?? String.Empty;
    string relativeDir = fileDir.Length == dir.Length ? fileDir : fileDir[(dir.Length + 1)..];
    string buildPath = Path.Combine(relativeDir, fileName).Replace('\\', '/');

    if (relativeDir.StartsWith("bin") || relativeDir.StartsWith("obj"))
        continue;

    string fileExtension = Path.GetExtension(filePath).ToLower();

    bool added = false;
    if (fileExtension == ".png")
    {
        mgcb.Append($"""
                     #begin {fileName}
                     /importer:TextureImporter
                     /processor:TextureProcessor
                     /processorParam:ColorKeyColor=255,0,255,255
                     /processorParam:ColorKeyEnabled=True
                     /processorParam:GenerateMipmaps=False
                     /processorParam:PremultiplyAlpha=True
                     /processorParam:ResizeToPowerOfTwo=False
                     /processorParam:MakeSquare=False
                     /processorParam:TextureFormat=Color
                     /build:{buildPath}


                     """);
        added = true;
    }
    else if (fileExtension == ".fx")
    {
        mgcb.Append($"""
                     #begin {fileName}
                     /importer:EffectImporter
                     /processor:EffectProcessor
                     /processorParam:DebugMode=Auto
                     /build:{buildPath}


                     """);
        added = true;
    }

    if (added)
    {
        string csDir = csBaseDir.Length == 0 ? relativeDir : relativeDir[(csBaseDir.Length + 1)..];
        int separatorIndex = csDir.IndexOf('\\');
        if (separatorIndex != -1)
            csDir = csDir[..separatorIndex];
        if (!assets.ContainsKey(csDir))
            assets[csDir] = [];
        assets[csDir].Add((Path.GetFileNameWithoutExtension(fileName), buildPath[..^fileExtension.Length]));
    }
}

StringBuilder cs = new();
cs.Append("""
          public static class Assets
          {
          """);
foreach (var kvp in assets)
{
    cs.Append($$"""
                  
                  public static class {{kvp.Key}}
                  {
              """);
    foreach ((string name, string path) in kvp.Value)
    {
        cs.Append($"""
                   
                           public const string {name} = "{path}";
                   """);
    }
    cs.Append("""
              
                  }
              """);
}
cs.Append("""
          
          }
          """);

File.WriteAllText("Assets.mgcb", mgcb.ToString());
File.WriteAllText("Assets.cs", cs.ToString());