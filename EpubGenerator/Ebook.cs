using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace EpubGenerator
{
    public class Ebook
    {
        readonly string tempPath;
        readonly string tempHash = DateTime.UtcNow.GetHashCode().ToString();

        public Ebook()
        {
            var currentLibFolder = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            tempPath = Path.Combine(currentLibFolder, "tempEpub", tempHash);
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
        }

        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Description { get; set; } = "";

        public event EventHandler<string> OnLog;

        public List<ChapterData> ChapterDatas { get; set; } = new List<ChapterData>();

        List<FileData> fileDatas { get; set; } = new List<FileData>();

        public void Write(string filePath, bool AutoGenerateHtmlContainerForChapters = true)
        {
            var chapCounter = 1;
            var volumes = ChapterDatas.Select(i => new { i.VolumeId, i.VolumeName }).Where(i => !string.IsNullOrWhiteSpace(i.VolumeName)).Distinct().ToList();
            var tocHtml = $"\n\t<nav epub:type=\"toc\">\n{Tabs(2)}<ol>";
            var htmlStart = ("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "\n<!DOCTYPE html>" +
                    "\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:epub=\"http://www.idpf.org/2007/ops\">" +
                    "\n<head><meta charset=\"utf-8\" /></head>" +
                    "\n<body>").Replace("\t", "  ");
            var htmlEnd = "\n</body>\n</html>";

            // creating toc.xhtml content
            {
                // adding Index page
                tocHtml += $"\n{Tabs(3)}<li>\n{Tabs(4)}<a href=\"toc.xhtml\">Contents</a>\n{Tabs(3)}</li>";

                // adding Cover page
                {
                    var coverImage = fileDatas.FirstOrDefault(i => (i.FileType == EnumFileType.JPEG || i.FileType == EnumFileType.PNG) && i.IsCoverImage == true);
                    var fData = new FileData()
                    {
                        FileName = "coverpage.xhtml",
                        FileType = EnumFileType.XHTML,
                    };
                    fData.PutString($"<div style=\"text-align:center;padding-top:20px\">{(coverImage == null ? "<div style=\"padding-top:130px\"><div>" : $"<img src=\"../img/{coverImage.FileName}\" />")}<h1>{Title}</h1><h3>{Author}</h3></div>");
                    fileDatas.Add(fData);

                    tocHtml += $"\n{Tabs(3)}<li>\n{Tabs(4)}<a href=\"coverpage.xhtml\">Cover</a>\n{Tabs(3)}</li>";
                }

                var chapterTabs = 3;
                if (volumes.Count > 1)
                    chapterTabs += 2;

                var chapterGroups = ChapterDatas.GroupBy(i => i.VolumeId);

                foreach (var gp in chapterGroups)
                {
                    var volumeNameValid = false;
                    if (volumes.FirstOrDefault(i => i.VolumeId == gp.Key) != null)
                        volumeNameValid = true;

                    if (volumes.Count > 1 && volumeNameValid)
                        tocHtml += $"\n{Tabs(3)}<li>\n{Tabs(4)}<a href=\"xhtml/{PadChapterCounter(chapCounter)}. {gp.FirstOrDefault().ChapterName.RemoveSpecialCharacters()}.xhtml\">{volumes.FirstOrDefault(i => i.VolumeId == gp.Key).VolumeName}</a>\n{Tabs(4)}<ol>";

                    foreach (var itm in gp)
                    {
                        var fileData = new FileData()
                        {
                            FileName = $"{PadChapterCounter(chapCounter)}. {itm.ChapterName.RemoveSpecialCharacters()}.xhtml",
                            FileType = EnumFileType.XHTML,
                            IsScripted = itm.ContainsScript,
                        };

                        if (AutoGenerateHtmlContainerForChapters)
                            fileData.PutString($"{htmlStart}\n<h2><u>{PadChapterCounter(chapCounter)}. {itm.ChapterName}</u></h2>\n{itm.ChapterContent.Replace("\t", "  ")}{htmlEnd}");
                        else
                            fileData.PutString($"\n<h2><u>{PadChapterCounter(chapCounter)}. {itm.ChapterName}</u></h2>\n{itm.ChapterContent}");

                        fileDatas.Add(fileData);

                        tocHtml += $"\n{Tabs(chapterTabs)}<li>\n{Tabs(chapterTabs + 1)}<a href=\"{fileData.FileName}\">{PadChapterCounter(chapCounter)}. {itm.ChapterName}</a>\n{Tabs(chapterTabs)}</li>";

                        chapCounter++;
                    }

                    if (volumes.Count > 1 && volumeNameValid)
                        tocHtml += $"\n{Tabs(4)}</ol>\n{Tabs(3)}</li>";
                }
            }
            tocHtml += $"\n{Tabs(2)}</ol>\n\t</nav>";

            // mimetype
            File.WriteAllText(Path.Combine(tempPath, "mimetype"), "application/epub+zip");

            // container.xml
            Directory.CreateDirectory(Path.Combine(tempPath, "META-INF"));
            File.WriteAllText(Path.Combine(tempPath, "META-INF", "container.xml"), @"<?xml version=""1.0"" encoding=""UTF-8""?>
<container xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"" version=""1.0"">
<rootfiles>
<rootfile full-path=""EPUB/package.opf"" media-type=""application/oebps-package+xml""/>
</rootfiles>
</container>");

            fileDatas = fileDatas.Where(i => i.FileBytes != null && i.FileBytes.Length > 0 && !string.IsNullOrWhiteSpace(i.FileName)).ToList();
            var manifest = "";
            var spine = "";
            var count = 1;
            var imgPath = Directory.CreateDirectory(Path.Combine(tempPath, "EPUB", "img")).FullName;
            var cssPath = Directory.CreateDirectory(Path.Combine(tempPath, "EPUB", "css")).FullName;
            var jsPath = Directory.CreateDirectory(Path.Combine(tempPath, "EPUB", "js")).FullName;
            var xhtmlPath = Directory.CreateDirectory(Path.Combine(tempPath, "EPUB", "xhtml")).FullName;
            var packagePath = Directory.CreateDirectory(Path.Combine(tempPath, "EPUB")).FullName;

            OnLog?.Invoke(this, "Generating toc file...");
            // toc.xhtml
            manifest += $"\n{Tabs(2)}<item id=\"toc\" properties=\"nav\" href=\"xhtml/toc.xhtml\" media-type=\"application/xhtml+xml\"/>";
            spine += $"\n{Tabs(2)}<itemref idref=\"toc\"/>";
            File.WriteAllText(Path.Combine(xhtmlPath, "toc.xhtml"), htmlStart + tocHtml.Replace("\t", "  ") + htmlEnd);

            OnLog?.Invoke(this, "Checking images...");
            // images
            var allImages = fileDatas.Where(i => i.FileType == EnumFileType.JPEG || i.FileType == EnumFileType.PNG);
            foreach (var itm in allImages)
            {
                File.WriteAllBytes(Path.Combine(imgPath, itm.FileName), itm.FileBytes);
                manifest += "\n\t\t<item id=\"img" + count + "\" href=\"img/" + itm.FileName + "\" media-type=\"image/" + (itm.FileType == EnumFileType.JPEG ? "jpeg" : "png") + "\"" + (itm.IsCoverImage ? " properties=\"cover-image\"" : "") + " />";
                count++;
            }

            count = 1;
            OnLog?.Invoke(this, "Checking stylesheets...");
            // css
            var allCSS = fileDatas.Where(i => i.FileType == EnumFileType.CSS);
            foreach (var itm in allCSS)
            {
                File.WriteAllBytes(Path.Combine(cssPath, itm.FileName), itm.FileBytes);
                manifest += "\n\t\t<item id=\"css" + count + "\" href=\"css/" + itm.FileName + "\" media-type=\"text/css\" />";
                count++;
            }

            count = 1;
            OnLog?.Invoke(this, "Checking scripts...");
            // js
            var allJS = fileDatas.Where(i => i.FileType == EnumFileType.JAVASCRIPT);
            foreach (var itm in allJS)
            {
                File.WriteAllBytes(Path.Combine(jsPath, itm.FileName), itm.FileBytes);
                manifest += "\n\t\t<item id=\"css" + count + "\" href=\"css/" + itm.FileName + "\" media-type=\"text/javascript\" />";
                count++;
            }

            count = 1;
            OnLog?.Invoke(this, "Checking content...");
            // xhtml
            var allXHTML = fileDatas.Where(i => i.FileType == EnumFileType.XHTML);
            foreach (var itm in allXHTML)
            {
                File.WriteAllBytes(Path.Combine(xhtmlPath, itm.FileName), itm.FileBytes);
                manifest += "\n\t\t<item id=\"p" + count + "\" href=\"xhtml/" + itm.FileName + "\" media-type=\"application/xhtml+xml\"" + (itm.IsScripted ? " properties=\"scripted\"" : "") + " />";
                spine += "\n\t\t<itemref idref=\"p" + count + "\" />";
                count++;
            }

            OnLog?.Invoke(this, "Generating package...");
            // package.opf
            var metadata = $"\n\t\t<dc:title>{Title}</dc:title>\n\t\t<dc:creator>{Author}</dc:creator>\n\t\t<dc:description>{Description}</dc:description>";
            var finalPackageContents = "<?xml version=\"1.0\"?>" +
                "\n<package xmlns=\"http://www.idpf.org/2007/opf\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" version=\"3.0\">" +
                "\n\t<metadata>" + metadata + "\n\t</metadata>" +
                "\n\t<manifest>" + manifest + "\n\t</manifest>" +
                "\n\t<spine>" + spine + "\n\t</spine>" +
                "\n</package>";
            File.WriteAllText(Path.Combine(packagePath, "package.opf"), finalPackageContents.Replace("\t", "  "));

            OnLog?.Invoke(this, "Compressing EPUB contents...");
            if (File.Exists(filePath))
                File.Delete(filePath);
            System.IO.Compression.ZipFile.CreateFromDirectory(tempPath, filePath);

            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
        }

        public void AddFile(FileData fileData)
        {
            if (fileData.FileType == EnumFileType.XHTML)
                throw new Exception("XHTML files are auto-generated. Please add chapters to ChapterDatas field for them to be generated automatically.");
            fileDatas.Add(fileData);
        }

        string PadChapterCounter(int chapterNumber)
        {
            var length = ChapterDatas.Count.ToString().Length;
            var retThis = chapterNumber.ToString();
            while (retThis.Length < length)
                retThis = "0" + retThis;
            return retThis;
        }

        string Tabs(int count = 1)
        {
            var tab = "\t";
            while (tab.Length < count)
                tab += "\t";
            return tab;
        }
    }
}
